using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class RoomPlacementRequest
    {
        /// <summary>
        /// The room that's requesting more rooms
        /// </summary>
        public Room Requester { get; init; }
        /// <summary>
        /// The director, which we use to check if we have enough credits, we also start the coroutine from here.
        /// </summary>
        public DungeonDirector DungeonDirector { get; init; }
        public bool IsComplete
        {
            get
            {
                return Credits <= 0 || doorToRooms.Count <= 0;
            }
        }
        public float Credits => DungeonDirector.Credits;
        private Dictionary<Door, WeightedCollection<DirectorCard>> doorToRooms = new();
        private Coroutine _coroutine;
        public RoomPlacementRequest(Room room, DungeonDirector director, ulong seed, WeightedCollection<DirectorCard> rooms)
        {
            var temp = new Xoroshiro128Plus(seed);
            Requester = room;
            DungeonDirector = director;
            foreach(var door in Requester.Doors)
            {
                if(!door.HasConnection)
                {
                    var copy = new WeightedCollection<DirectorCard>(rooms);
                    copy.SetSeed(temp.NextUlong);
                    doorToRooms.Add(door, copy);
                }
            }
        }

        public void StartCoroutine()
        {
            _coroutine = DungeonDirector.StartCoroutine(PlacementLoop());
        }

        private IEnumerator PlacementLoop()
        {
            yield return null;
            //Try placing adjacent rooms as long as there are open doors
            while(!IsComplete)
            {
                //If credits are consumed, stop early
                if (Credits < 0)
                {
                    StopCoroutine();
                    yield break;
                }

                yield return null;
                List<Door> doors = new List<Door>();
                List<Door> doorsToRemoveBecauseWeRanOutOfRooms = new List<Door>();
                foreach(var kvp in doorToRooms)
                {
#if DEBUG
                    if(DungeonDirector.slowGeneration)
                    {
                        yield return new WaitForSeconds(DungeonDirector.slowGenerationWait);
                    }
#endif
                    var door = kvp.Key;
                    var roomCards = kvp.Value;
                    doors.Add(door);
                    if(door.HasConnection)
                    {
                        continue;
                    }
                    int choiceIndex = roomCards.NextIndex();
                    if(choiceIndex == -1)
                    {
                        doorsToRemoveBecauseWeRanOutOfRooms.Add(door);
                        continue;
                    }
                    var room = roomCards[choiceIndex];

                    if(!DungeonDirector.TryPlaceRoom(room.value, door, true))
                    {
                        roomCards.RemoveAt(choiceIndex);
                    }
                }
                foreach(Door door in doors)
                {
                    if(door.HasConnection || !door.IsOpen)
                    {
                        doorToRooms.Remove(door);
                    }
                }
                foreach(Door door in doorsToRemoveBecauseWeRanOutOfRooms)
                {
                    doorToRooms.Remove(door);
                }
            }
            StopCoroutine();
            yield break;
        }


        private void StopCoroutine()
        {
            DungeonDirector.StopCoroutine(_coroutine);
        }
    }
}