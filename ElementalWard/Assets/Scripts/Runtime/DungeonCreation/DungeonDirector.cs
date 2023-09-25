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
        private WeightedSelection<DungeonDeck.Card> _entrywayCards;
        private WeightedSelection<DungeonDeck.Card> _roomCards;
        private WeightedSelection<DungeonDeck.Card> _bossRoomCards;
        private Queue<RoomPlacementRequest> _placementRequestQueue = new Queue<RoomPlacementRequest>();
        private RoomPlacementRequest _currentRequest;
        private GameObject thing;
        private void Awake()
        {
            var seed = useCustomSeed ? _customSeed : (ulong)DateTime.Now.Ticks;
            Debug.Log("Dungeon RNG Seed: " + seed);
            _dungeonRNG = new Xoroshiro128Plus(seed);
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
            _roomCards = _dungeonDeck.GenerateSelectionFromRoomPool(_dungeonDeck.roomCards);

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
                thing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                thing.transform.position = _currentRequest.Requester.transform.position + Vector3.up * 15;
            }

            if((bool)(_currentRequest?.IsComplete))
            {
                _currentRequest = null;
                Destroy(thing);
                return;
            }
        }

        private void PlaceEntryway()
        {
            var entryway = _entrywayCards.Evaluate(_dungeonRNG.NextNormalizedFloat);
            var gameObject = Instantiate(entryway.prefab, transform);
            Room room = gameObject.GetComponent<Room>();
            var request = new RoomPlacementRequest(room, this, _roomCards, _dungeonRNG.NextUlong);
            _placementRequestQueue.Enqueue(request);
        }

        public bool TryPlaceRoom(DungeonDeck.Card card, Door door)
        {
            GameObject instantiatedObject = Instantiate(card.prefab, transform);
            Room instantiatedRoom = instantiatedObject.GetComponent<Room>();
            /*Door instantiatedDoor = instantiatedRoom.GetFirstAvailableDoor();
            if(instantiatedDoor == null)
            {
                return false;
            }*/

            for (int i = 0; i < _dungeonRNG.RangeInt(0, 4); i++)
            {
                instantiatedObject.transform.rotation = Quaternion.Euler(0, instantiatedObject.transform.rotation.eulerAngles.y + 90, 0);
            }
            Door closestDoor = null;
            for(int i = 0; i < instantiatedRoom.Doors.Length; i++)
            {
                if (Mathf.Approximately(instantiatedRoom.Doors[i].transform.localEulerAngles.y, door.transform.localEulerAngles.y))
                {
                    closestDoor = instantiatedRoom.Doors[i];
                    break;
                }
            }

            instantiatedRoom.transform.position = door.transform.position - closestDoor.transform.position;
            

            Physics.SyncTransforms();
            instantiatedRoom.CalculateBounds();
            /*instantiatedObject.transform.position = door.transform.position - instantiatedDoor.transform.position;

            float angle = Quaternion.Angle(door.transform.rotation, Quaternion.identity);
            Debug.Log(angle);
            throw new NullReferenceException();
            instantiatedObject.transform.RotateAround(instantiatedDoor.transform.position, Vector3.up, angle);*/
            
            /*var fwdA = door.transform.rotation * Vector3.forward;
            var fwdB = adjacentDoor.transform.rotation * Vector3.forward;
            var angleA = Mathf.Atan2(fwdA.x, fwdA.z) * Mathf.Rad2Deg;
            var angleB = Mathf.Atan2(fwdB.x, fwdB.z) * Mathf.Rad2Deg;
            var angleDiff = Mathf.Round(Mathf.DeltaAngle(angleA, angleB));
            Debug.Log(angleDiff);
            if(Mathf.Abs(angleDiff) != 90)
            {
                angleDiff += 180;
            }
            else if(Mathf.Abs(angleDiff) != 90 && Mathf.Abs(angleDiff) != 270)
            {
                angleDiff += 180;
            }
            
            roomObject.transform.RotateAround(adjacentDoor.transform.position, Vector3.up, angleDiff);*/
            //throw new NullReferenceException();
            //throw new NullReferenceException();
            /*
            var bounds = room.RawBoundingBox;
            var center = roomObject.transform.rotation * bounds.center;
            bounds.center = center;
            var size = roomObject.transform.rotation * bounds.size;
            bounds.size = size;
            room.RawBoundingBox = bounds;
            */
            
            if(IntersectsWithAnotherRoom(instantiatedRoom, door.ParentRoom))
            {
                throw new NullReferenceException();
                Destroy(instantiatedObject);
                return false;
            }
            //Connect both rooms;
            closestDoor.ConnectedDoor = door;
            door.ConnectedDoor = closestDoor;
            FindConnections(instantiatedRoom);

            Credits -= card.cardCost;
            var request = new RoomPlacementRequest(instantiatedRoom, this, _roomCards, _dungeonRNG.NextUlong);
            _placementRequestQueue.Enqueue(request);
            return true;
        }

        private bool IntersectsWithAnotherRoom(Room roomToCheck, params Room[] roomsToIgnore)
        {
            Physics.SyncTransforms();
            Bounds bounds = roomToCheck.WorldBoundingBox;
            //bounds.Expand(2);
            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, Physics.AllLayers);

            List<Room> rooms = colliders.Select(x => x.GetComponentInParent<Room>()).Distinct().ToList();

            foreach(Room room in rooms)
            {
                Debug.Log($"Checking if {room} intersects with {roomToCheck}", roomToCheck);
                if(room == roomToCheck)
                {
                    Debug.Log($"{room} and {roomToCheck} are the same, continuing", room);
                    continue;
                }
                if (roomsToIgnore.Contains(room))
                {
                    Debug.Log($"{room} is inside the rooms to ignore array, likely its the requester., continuing", room);
                    continue;
                }
                Debug.Log($"{room} intersects with {roomToCheck}, returning true.");
                return true;
            }
            return false;
        }

        private void FindConnections(Room room)
        {

            Physics.SyncTransforms();
            foreach (var door in room.Doors)
            {
                if (door.HasConnection || !door.IsOpen)
                {
                    continue;
                }
                Debug.Log(door.transform, door.transform);

                Bounds bounds = UnityUtil.CalculateColliderBounds(door.gameObject, true);
                bounds.Expand(2f);

                Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Collide);
                List<Door> doorsFromCollision = colliders.Select(x => x.GetComponentInParent<Door>()).Where(d => d != door).Distinct().ToList();
                Door firstOrDefault = doorsFromCollision.FirstOrDefault();
                if(firstOrDefault)
                {
                    Debug.Log($"{doorsFromCollision.Count}: {firstOrDefault}", firstOrDefault);
                    door.ConnectedDoor = firstOrDefault;
                    continue;
                }
            }
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
