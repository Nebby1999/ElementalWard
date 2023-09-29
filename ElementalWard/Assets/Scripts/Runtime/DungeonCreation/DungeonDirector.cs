using Cinemachine.Utility;
using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public class DungeonDirector : MonoBehaviour
    {
        private static readonly IntMinMax DUNGEON_BASE_SIZE = new IntMinMax(25, 75);
        private static readonly float DUNGEON_BASE_CREDITS = 750f;
        public BoundsInt DungeonSize { get; private set; }
        public float Credits { get; private set; }
        public bool GenerationComplete => _currentRequest == null && _placementRequestQueue.Count <= 0;
        [SerializeField] private DungeonDeck _dungeonDeck;
        [SerializeField] private ulong _dungeonFloor;
        [SerializeField] private bool useCustomSeed;
        [SerializeField] private ulong _customSeed;

        private Xoroshiro128Plus _dungeonRNG;
        private WeightedCollection<DungeonDeck.Card> _entrywayCards;
        private WeightedCollection<DungeonDeck.Card> _roomCards;
        private WeightedCollection<DungeonDeck.Card> _bossRoomCards;
        private Queue<RoomPlacementRequest> _placementRequestQueue = new Queue<RoomPlacementRequest>();
        private RoomPlacementRequest _currentRequest;
        private ulong seed;

        private void Awake()
        {
            seed = useCustomSeed ? _customSeed : (ulong)DateTime.Now.Ticks;
            Debug.Log("Dungeon RNG Seed: " + seed);
            _dungeonRNG = new Xoroshiro128Plus(seed);
        }

        [ContextMenu("Recreate")]
        private void Regen()
        {
            _dungeonRNG.ResetSeed(seed);
            Start();
        }
        private void Start()
        {
            float floorMultiplier = 1 + _dungeonFloor / 10;

            int[] sizes = new int[3];
            for(int i = 0; i < sizes.Length; i++)
            {
                sizes[i] = (int)(DUNGEON_BASE_SIZE.GetRandomRange(_dungeonRNG) * floorMultiplier * transform.lossyScale[i]);
            }
            Array.Sort(sizes);
            int xIndex = _dungeonRNG.RangeInt(1, sizes.Length);
            int zIndex = xIndex == 1 ? 2 : 1;

            int sizeX = sizes[xIndex];
            int sizeZ = sizes[zIndex];
            int sizeY = sizes[0] / 2;

            var size = new Vector3Int(sizeX, sizeY, sizeZ);

            var posX = Mathf.FloorToInt(transform.position.x) - size.x / 2;
            var posY = Mathf.FloorToInt(transform.position.x);
            var posZ = Mathf.FloorToInt(transform.position.x);
            var pos = new Vector3Int(posX, posY, posZ);

            DungeonSize = new BoundsInt(pos, size);

            Credits = DUNGEON_BASE_CREDITS * floorMultiplier;

            _entrywayCards = _dungeonDeck.GenerateSelection(_dungeonDeck.entrywayRoomCards);
            _entrywayCards.SetSeed(_dungeonRNG.NextUlong);
            _roomCards = _dungeonDeck.GenerateSelectionFromPool(_dungeonDeck.roomCards);
            _roomCards.SetSeed(_dungeonRNG.NextUlong);

            PlaceEntryway();
        }

        private void FixedUpdate()
        {
            if(GenerationComplete)
            {
                return;
            }

            if(_currentRequest == null && _placementRequestQueue.TryDequeue(out var request))
            {
                _currentRequest = request;
                _currentRequest.StartCoroutine();
            }

            if((bool)(_currentRequest?.IsComplete))
            {
                _currentRequest = null;
                return;
            }
        }

        private void PlaceEntryway()
        {
            var entryway = _entrywayCards.Next();
            var gameObject = Instantiate(entryway.prefab, transform);
            Room room = gameObject.GetComponent<Room>();
            var request = new RoomPlacementRequest(room, this, _dungeonRNG.NextUlong, _roomCards);
            _placementRequestQueue.Enqueue(request);
        }

        public bool TryPlaceRoom(DungeonDeck.Card card, Door door)
        {
            GameObject instantiatedObject = Instantiate(card.prefab, door.ParentRoom.transform.position, door.ParentRoom.transform.rotation, transform);
            instantiatedObject.transform.position = door.ParentRoom.transform.position;
            Room instantiatedRoom = instantiatedObject.GetComponent<Room>();
            Door instantiatedDoor = instantiatedRoom.GetRandomAvailableDoor(_dungeonRNG);

            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            primitive.parent = instantiatedDoor.transform;
            primitive.localPosition = Vector3.zero;

            Vector3 pos = door.transform.position - instantiatedDoor.transform.position;
            instantiatedObject.transform.position += pos;

            if (instantiatedDoor.transform.forward != -door.transform.forward)
            {
                float angle = Vector3.SignedAngle(door.transform.forward, instantiatedDoor.transform.forward, Vector3.up);
                angle = MathF.Round(angle);
                if (angle == 0)
                {
                    angle = 180;
                }
                instantiatedObject.transform.RotateAround(instantiatedDoor.transform.position, Vector3.up, angle);
            }

            Physics.SyncTransforms();
            instantiatedRoom.CalculateBounds();
            var b = instantiatedRoom.RawBoundingBox;
            b.Expand(-0.5f);
            instantiatedRoom.RawBoundingBox = b;

            var (newConnections, roomsToIgnore) = FindConnections(instantiatedRoom);
            roomsToIgnore.Add(door.ParentRoom);

            if(IntersectsWithAnotherRoom(instantiatedRoom, roomsToIgnore.ToArray()))
            {
                foreach(var (doorA, doorB) in newConnections)
                {
                    doorA.ConnectedDoor = null;
                    doorB.ConnectedDoor = null;
                }
                Destroy(instantiatedObject, 0.5f);
                return false;
            }

            //Connect Doors
            instantiatedDoor.ConnectedDoor = door;
            door.ConnectedDoor = instantiatedDoor;

            //Create request for new room
            RoomPlacementRequest request = new RoomPlacementRequest(instantiatedRoom, this, _dungeonRNG.NextUlong, _roomCards);
            _placementRequestQueue.Enqueue(request);

            return true;
        }

        private bool IntersectsWithAnotherRoom(Room roomToCheck, params Room[] roomsToIgnore)
        {
            Bounds bounds = roomToCheck.WorldBoundingBox;
            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, Physics.AllLayers);

            List<Room> rooms = colliders.Select(x => x.GetComponentInParent<Room>()).Where(r => r && r != roomToCheck && !roomsToIgnore.Contains(r)).Distinct().ToList();
            //Debug.Log(rooms.Count);
            return rooms.Count > 0;
        }

        private (List<(Door, Door)>, List<Room>) FindConnections(Room room)
        {
            List<(Door, Door)> newConnections = new();
            List<Room> rooms = new List<Room>();
            foreach (var door in room.Doors)
            {
                if (door.HasConnection || !door.IsOpen)
                {
                    continue;
                }

                Bounds bounds = door.TriggerCollider.bounds;
                bounds.Expand(0.5f);

                Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Collide);
                List<Door> doorsFromCollision = colliders.Where(c => c.isTrigger && c != door.TriggerCollider).Select(c => c.GetComponent<Door>()).Distinct().ToList();
                Door firstOrDefault = doorsFromCollision.FirstOrDefault();
                if(firstOrDefault && Mathf.Approximately(firstOrDefault.transform.position.sqrMagnitude, door.transform.position.sqrMagnitude))
                {
                    door.ConnectedDoor = firstOrDefault;
                    firstOrDefault.ConnectedDoor = door;
                    newConnections.Add((door, firstOrDefault));
                    if(!rooms.Contains(firstOrDefault.ParentRoom))
                        rooms.Add(firstOrDefault.ParentRoom);
                    continue;
                }
            }
            return (newConnections, rooms);
        }

        private void OnDrawGizmos()
        {
            if(!Application.isPlaying)
            {
                return;
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(DungeonSize.center, DungeonSize.size);
        }
    }
}
