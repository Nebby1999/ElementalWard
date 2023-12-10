using Cinemachine.Utility;
using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public class DungeonDirector : MonoBehaviour
    {
        private static readonly FloatMinMax DUNGEON_BASE_SIZE = new FloatMinMax(20, 40);
        private static readonly FloatMinMax DUNGEON_BASE_CREDITS = new FloatMinMax(100, 200);
        public Bounds DungeonSize { get; private set; }
        public float Credits { get; private set; }
        public bool GenerationComplete { get; private set; }
        public ReadOnlyCollection<Room> InstantiatedRooms => new ReadOnlyCollection<Room>(_instantiatedRooms);
        public event Action<DungeonDirector> OnDungeonGenerationComplete;
        [SerializeField] private DungeonCardDeck _dungeonDeck;
        public ulong DungeonFloor { get; private set; }
#if DEBUG
        public bool slowGeneration;
        [Range(0, 10)]
        public float slowGenerationWait;
#endif

        private Xoroshiro128Plus _dungeonRNG;
        private WeightedCollection<DirectorCard> _entrywayCards;
        private WeightedCollection<DirectorCard> _roomCards;
        private WeightedCollection<DirectorCard> _bossRoomCards;
        private Queue<RoomPlacementRequest> _placementRequestQueue = new Queue<RoomPlacementRequest>();
        private RoomPlacementRequest _currentRequest;
        private List<Room> _instantiatedRooms = new List<Room>();

        private void Awake()
        {
            _dungeonRNG = new Xoroshiro128Plus(DungeonManager.Instance.rng.NextUlong);
        }

        private void Start()
        {
            _dungeonRNG = new Xoroshiro128Plus(DungeonManager.Instance.rng.NextUlong);
            DungeonFloor = DungeonManager.Instance.DungeonFloor;
            float sizeMultiplier = DungeonManager.Instance.DifficultyCoefficient * 1.1f;
            float creditMultiplier = DungeonManager.Instance.DifficultyCoefficient;

            float[] sizes = new float[3];
            for(int i = 0; i < sizes.Length; i++)
            {
                sizes[i] = (float)(DUNGEON_BASE_SIZE.GetRandomRange(_dungeonRNG) * sizeMultiplier * transform.lossyScale[i]);
            }
            Array.Sort(sizes);
            int xIndex = _dungeonRNG.RangeInt(1, sizes.Length);
            int zIndex = xIndex == 1 ? 2 : 1;

            float sizeX = sizes[xIndex];
            float sizeZ = sizes[zIndex];
            float sizeY = sizes[0];

            var size = new Vector3(sizeX, sizeY, sizeZ);

            //This makes the beginning room to be roughly at the center of one of the bound's walls.
            var posX = Mathf.CeilToInt(transform.position.x);
            var posY = Mathf.CeilToInt(transform.position.y);
            var posZ = Mathf.CeilToInt(transform.position.z + sizeZ / 2);
            var pos = new Vector3(posX, posY, posZ);

            DungeonSize = new Bounds(pos, size);

            Credits = DUNGEON_BASE_CREDITS.GetRandomRange(_dungeonRNG) * creditMultiplier;

            _entrywayCards = _dungeonDeck.GenerateSelection(_dungeonDeck.entrywayRoomCards);
            _entrywayCards.SetSeed(_dungeonRNG.NextUlong);
            _roomCards = _dungeonDeck.GenerateSelectionFromPools(_dungeonDeck.roomCards);
            _roomCards.SetSeed(_dungeonRNG.NextUlong);
            _bossRoomCards = _dungeonDeck.GenerateSelection(_dungeonDeck.bossRoomCards);
            _bossRoomCards.SetSeed(_dungeonRNG.NextUlong);

            PlaceEntryway();
        }

        private void FixedUpdate()
        {
            if (GenerationComplete)
                return;

            if(_currentRequest == null && _placementRequestQueue.TryDequeue(out var request))
            {
                _currentRequest = request;
                _currentRequest.StartCoroutine();
            }

            if(_currentRequest?.IsComplete ?? false)
            {
                _currentRequest = null;
                return;
            }

            if(Credits <= 0 && _currentRequest == null && _placementRequestQueue.Count <= 0)
            {
                GenerationComplete = true;
                SpawnObligatoryRooms();
            }
        }

        private void SpawnObligatoryRooms()
        {
            var bossRoom = _bossRoomCards.Next();
            List<Door> doors = GetDoorsWithNoConnections().OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).Reverse().ToList();

            Door toRemove = null;
            try
            {
                foreach(var door in doors)
                {
                    if(TryPlaceRoom(bossRoom, door, false))
                    {
                        toRemove = door;
                        break;
                    }
                }
            }catch(Exception) { enabled = false; }
            doors.Remove(toRemove);

            foreach(var door in doors)
            {
                door.IsOpen = door.HasConnection;
            }
            GenerationComplete = true;
            OnDungeonGenerationComplete?.Invoke(this);
        }

        public List<Door> GetDoorsWithNoConnections()
        {
            List<Door> result = new();
            foreach (var room in _instantiatedRooms)
            {
                foreach (var door in room.Doors)
                {
                    if (!door.HasConnection && door.IsOpen)
                    {
                        result.Add(door);
                    }
                }
            }
            return result;
        }

        private void PlaceEntryway()
        {
            var entryway = _entrywayCards.Next();
            var placementRule = new PlacementRule(transform)
            {
                maxDistance = entryway.maximumSpawnDistance,
                minDistance = entryway.minimumSpawnDistance,
                placement = PlacementRule.DirectPlacement
            };

            SpawnRequest spawnRequest = new SpawnRequest(entryway.spawnCard, placementRule, _dungeonRNG);
            var gameObject = DungeonManager.Instance.TrySpawnObject(spawnRequest);

            Transform instantiatedTransform = gameObject.transform;
            instantiatedTransform.parent = transform;
            instantiatedTransform.localScale = Vector3.one;
            Room room = gameObject.GetComponent<Room>();
            Physics.SyncTransforms();
            room.CalculateBounds();
            var b = room.RawBoundingBox;
            b.Expand(-1f);
            room.RawBoundingBox = b;
            _instantiatedRooms.Add(room);
            var roomRequest = new RoomPlacementRequest(room, this, _dungeonRNG.NextUlong, _roomCards);
            _placementRequestQueue.Enqueue(roomRequest);
        }


        public GameObject TryPlaceRoom(DirectorCard card, Door door, bool respectBoundary)
        {
            var placementRule = new PlacementRule(door.ParentRoom.transform)
            {
                maxDistance = card.maximumSpawnDistance,
                minDistance = card.minimumSpawnDistance,
                placement = PlacementRule.DirectPlacement
            };

            SpawnRequest spawnRequest = new SpawnRequest(card.spawnCard, placementRule, _dungeonRNG);
            GameObject instantiatedObject = DungeonManager.Instance.TrySpawnObject(spawnRequest);

            Transform instantiatedTransform = instantiatedObject.transform;
            instantiatedTransform.parent = transform;
            instantiatedTransform.localScale = Vector3.one;
            instantiatedObject.transform.position = door.ParentRoom.transform.position;
            Room instantiatedRoom = instantiatedObject.GetComponent<Room>();
            Door instantiatedDoor = instantiatedRoom.GetRandomAvailableDoor(_dungeonRNG);

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
            b.Expand(-1f);
            instantiatedRoom.RawBoundingBox = b;
            Physics.SyncTransforms();
            instantiatedRoom.CalculateBounds();
            b = instantiatedRoom.RawBoundingBox;
            b.Expand(-1f);
            instantiatedRoom.RawBoundingBox = b;

            var (newConnections, roomsToIgnore) = FindConnections(instantiatedRoom);
            roomsToIgnore.Add(door.ParentRoom);

            if(IsPositionInvalid(instantiatedRoom, respectBoundary, roomsToIgnore.ToArray()))
            {
                foreach(var (doorA, doorB) in newConnections)
                {
                    doorA.ConnectedDoor = null;
                    doorB.ConnectedDoor = null;
                }
#if DEBUG
                if (slowGeneration)
                    Destroy(instantiatedObject, slowGenerationWait / 2);
                else
                    Destroy(instantiatedObject);
#else
                Destroy(instantiatedObject);
#endif
                return null;
            }

            instantiatedDoor.ConnectedDoor = door;
            door.ConnectedDoor = instantiatedDoor;

            _instantiatedRooms.Add(instantiatedRoom);
            Credits -= card.cardCost;

            //Create request for new room
            if(Credits > 0)
            {
                RoomPlacementRequest request = new RoomPlacementRequest(instantiatedRoom, this, _dungeonRNG.NextUlong, _roomCards);
                _placementRequestQueue.Enqueue(request);
            }

            return instantiatedObject;
        }

        private bool IsPositionInvalid(Room roomToCheck, bool respectBoundary, params Room[] roomsToIgnore)
        {
            Bounds bounds = roomToCheck.WorldBoundingBox;
            if(respectBoundary && !DungeonSize.Intersects(bounds))
            {
                return true;
            }
            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, Physics.AllLayers);

            List<Room> rooms = colliders.Select(x => x.GetComponentInParent<Room>()).Where(r => r && r != roomToCheck && !roomsToIgnore.Contains(r)).Distinct().ToList();
            //Debug.Log(rooms.Count);

            if (rooms.Count > 0)
                return true;

            foreach(Room room in _instantiatedRooms)
            {
                if (roomsToIgnore.Contains(room))
                    continue;

                if(room.WorldBoundingBox.Intersects(roomToCheck.WorldBoundingBox))
                {
                    return true;
                }
            }
            return false;
        }

        private (List<(Door, Door)>, List<Room>) FindConnections(Room room)
        {
            List<(Door, Door)> newConnections = new();
            List<Room> rooms = new List<Room>();
            foreach (var door in room.Doors)
            {
                Physics.SyncTransforms();

                if (door.HasConnection)
                {
                    continue;
                }

                Bounds bounds = door.TriggerCollider.bounds;
                bounds.Expand(3f);

                Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Collide);
                List<Door> doorsFromCollision = colliders.Where(c => c.isTrigger && c != door.TriggerCollider).Select(c => c.GetComponent<Door>()).Distinct().ToList();

                foreach(var d in doorsFromCollision)
                {
                    if(d && !d.HasConnection && Mathf.Approximately(d.transform.position.sqrMagnitude, door.transform.position.sqrMagnitude))
                    {
                        door.ConnectedDoor = d;
                        d.ConnectedDoor = door;
                        newConnections.Add((door, d));
                        if(!rooms.Contains(d.ParentRoom))
                            rooms.Add(d.ParentRoom);

                        break;
                    }
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
