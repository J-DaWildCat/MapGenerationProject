using System;
using System.Collections.Generic;

namespace MapGenerationProject
{
    public class MapGeneration
    {
        static void Main(string[] args)
        {
            for (int v = 0; v < 10; v++)
            {
                Console.WriteLine("Map number " + v);

                MapGeneration mapGeneration = new MapGeneration();

                DateTime before = DateTime.Now;
                mapGeneration.CreateMap();
                mapGeneration.ShowMapTerminal();

                DateTime after = DateTime.Now;

                TimeSpan timeSpent = after - before;

                Console.WriteLine("Time spent =  " + timeSpent.TotalSeconds);
                Console.WriteLine();
            }
        }

        //Values are the number of rooms in a map.
        //Why value? I'll maybe add later bigger / smaller rooms with different value
        public int maxMapValue = 25;
        
        //When creating a map with the Special rooms and the path to them, if the number of room is low
        //that means that the paths are the same to one room to another in a certain extend.
        //So putting a minimum number of rooms without the other random one added makes the specials rooms well splited
        public int minMapValueWithoutRooms = 12;

        //Size of the grid in which the room is created
        public int mapSize = 20;
            
        //Basicly to make the specials rooms not be next to each other
        public int maxDistanceBetweenRoom = 5;
        public int minDistanceBetweenRoom = 3;

        //Used to verify a number of room in a X,X dimension square
        public int maxRoomInSquare = 7;
        public int squareSize = 3;

        //Used when a random room is created. % of chance of continuing creating random rooms form the new one
        public int randomCorridorPourcentage = 50;
        public int maxRoomInCorridor = 6;

        //The layout is to do all maths the easiest way possible
        //The list of Rooms is to save the rooms and navigate through them faster
        public int[,] mapLayout;
        List<Room> map;

        //Those are the IDs of the rooms
        public int emptyRoom = 0;
        public int spawnRoom = 1;
        public int shopRoom = 2;
        public int specialRoom = 3;
        public int bossRoom = 4;
        public int normalRoom = 5;

        //For the algorythm
        private int currentMapValue = 0;
        public void CreateMap()
        {
            bool abortCreation = false;

            //Do while I create a map that doesn't reach my standards
            do
            {
                try
                {
                    //Creates variables

                    mapLayout = new int[mapSize, mapSize];
                    currentMapValue = 0;
                    map = new List<Room>();
                    double distance;
                    double distanceFromSpawn;

                    //Create Spawn room on grid

                    int ySpawn = getRandom(mapSize - 1);
                    int xSpawn = getRandom(mapSize - 1);

                    mapLayout[xSpawn, ySpawn] = spawnRoom;
                    map.Add(new Room(xSpawn, ySpawn, spawnRoom));
                    currentMapValue++;

                    //Create Shop room on grid

                    int yShop;
                    int xShop;
                    do
                    {
                        yShop = getRandom(mapSize - 1);
                        xShop = getRandom(mapSize - 1);
                        distance = Math.Sqrt(Math.Pow(yShop - ySpawn, 2) + Math.Pow(xShop - xSpawn, 2));
                    } while (distance < minDistanceBetweenRoom || distance > maxDistanceBetweenRoom);

                    mapLayout[xShop, yShop] = shopRoom;
                    map.Add(new Room(xShop, yShop, shopRoom));
                    currentMapValue++;

                    //Create special room on grid

                    int ySpecial;
                    int xSpecial;
                    do
                    {
                        ySpecial = getRandom(mapSize - 1);
                        xSpecial = getRandom(mapSize - 1);
                        distance = Math.Sqrt(Math.Pow(ySpecial - ySpawn, 2) + Math.Pow(xSpecial - xSpawn, 2));
                    } while (distance < minDistanceBetweenRoom || distance > maxDistanceBetweenRoom);

                    mapLayout[xSpecial, ySpecial] = specialRoom;
                    map.Add(new Room(xSpecial, ySpecial, specialRoom));
                    currentMapValue++;                                     

                    //Creates room between shop to boss

                    if (isProbabilityReached(50, 100))
                    {
                        //Create Boss room on grid from shop

                        int yBoss;
                        int xBoss;

                        do
                        {
                            yBoss = getRandom(mapSize - 1);
                            xBoss = getRandom(mapSize - 1);
                            distance = Math.Sqrt(Math.Pow(yBoss - yShop, 2) + Math.Pow(xBoss - xShop, 2));
                            distanceFromSpawn = Math.Sqrt(Math.Pow(yBoss - ySpawn, 2) + Math.Pow(xBoss - xSpawn, 2));
                        } while (distance < minDistanceBetweenRoom || distance > maxDistanceBetweenRoom || distanceFromSpawn < minDistanceBetweenRoom || distanceFromSpawn > maxDistanceBetweenRoom);

                        mapLayout[xBoss, yBoss] = bossRoom;
                        map.Add(new Room(xBoss, yBoss, bossRoom));
                        currentMapValue++;

                        //Creates room between Spawn and shop

                        ConnectRooms(xShop, yShop, xSpawn, ySpawn);

                        //Creates room between Spawn and special

                        ConnectRooms(xSpecial, ySpecial, xSpawn, ySpawn);

                        //Creates room between boss and shop

                        ConnectRooms(xBoss, yBoss, xShop, yShop);
                    }
                    else
                    {//Create Boss room on grid from other room

                        int yBoss;
                        int xBoss;

                        do
                        {
                            yBoss = getRandom(mapSize - 1);
                            xBoss = getRandom(mapSize - 1);
                            distance = Math.Sqrt(Math.Pow(yBoss - ySpecial, 2) + Math.Pow(xBoss - xSpecial, 2));
                            distanceFromSpawn = Math.Sqrt(Math.Pow(yBoss - ySpawn, 2) + Math.Pow(xBoss - xSpawn, 2));
                        } while (distance < minDistanceBetweenRoom || distance > maxDistanceBetweenRoom || distanceFromSpawn < minDistanceBetweenRoom || distanceFromSpawn > maxDistanceBetweenRoom);

                        mapLayout[xBoss, yBoss] = bossRoom;
                        map.Add(new Room(xBoss, yBoss, bossRoom));
                        currentMapValue++;

                        //Creates room between Spawn and shop

                        ConnectRooms(xShop, yShop, xSpawn, ySpawn);

                        //Creates room between Spawn and special

                        ConnectRooms(xSpecial, ySpecial, xSpawn, ySpawn);

                        //Creates room between boss and special

                        ConnectRooms(xBoss, yBoss, xSpecial, ySpecial);
                    }
                   
                    if (currentMapValue > minMapValueWithoutRooms)
                    {
                        //Creates dead ends while i don't have enough rooms
                        while (currentMapValue < maxMapValue)
                        {
                            currentMapValue += CreateRandomRoom(maxMapValue-currentMapValue);
                        }

                        //Verify if map too much cruched

                        if(VerifyRoomDisposition(squareSize,maxRoomInSquare))
                        {
                            if (VerifyCoridorDisposition(maxRoomInCorridor))
                            {
                                abortCreation = false;
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                }
                //Create a new map !
                catch (InvalidOperationException exception)
                {
                    abortCreation = true;
                }
            } while (abortCreation);
        }


        //Creates normal rooms between special rooms
        private void ConnectRooms(int xRoom, int yRoom, int xCurrentRoom, int yCurrentRoom)
        {
            int xStep, yStep;

            //What is the direction I should go?
            if (xCurrentRoom > xRoom) //Top
            {
                xStep = -1;
            }

            else
                xStep = 1;  //Bottom

            if (yCurrentRoom > yRoom) //Left
                yStep = -1;

            else            //Right
                yStep = 1;  

            //While i'm not at the room
            while (xCurrentRoom != xRoom || yCurrentRoom != yRoom)
            {
                bool right = false;
                bool left = false;
                bool top = false;
                bool bottom = false;

                //50% of goind in X and 50% of going Y
                if (isProbabilityReached(50, 100)) //Goes X
                {
                    if (xCurrentRoom != xRoom)
                    {
                        if(xStep > 0)
                        {
                            bottom = true;
                        }
                        else
                        {
                            top = true;
                        }
                        xCurrentRoom += xStep;
                    }
                }
                else //Goes Y
                {
                    if (yCurrentRoom != yRoom)
                    {
                        if (yStep > 0)
                        {
                            right = true;
                        }
                        else
                        {
                            left = true;
                        }
                        yCurrentRoom += yStep;
                    }
                }

                //If i'm an empty room or I'm arrived at destination
                if (mapLayout[xCurrentRoom, yCurrentRoom] == emptyRoom || (xCurrentRoom == xRoom && yCurrentRoom == yRoom))
                {
                    //Create the door connection from the last room to the current one
                    if (right)
                    {
                        Room room = map.Find(r => (r.X == xCurrentRoom ) && (r.Y == yCurrentRoom - 1));
                        room.HasDoorRight = true;
                    }
                    else if (left)
                    {
                        Room room = map.Find(r => (r.X == xCurrentRoom ) && (r.Y == yCurrentRoom + 1));
                        room.HasDoorLeft = true;
                    }
                    else if (top)
                    {
                        Room room = map.Find(r => (r.X == xCurrentRoom + 1) && (r.Y == yCurrentRoom ));
                        room.HasDoorTop = true;
                    }
                    else if (bottom)
                    {
                        Room room = map.Find(r => (r.X == xCurrentRoom - 1) && (r.Y == yCurrentRoom ));
                        room.HasDoorBottom = true;
                    }

                    //If current room is empty, create it with the good doors
                    if (mapLayout[xCurrentRoom, yCurrentRoom] == emptyRoom)
                    {
                        mapLayout[xCurrentRoom, yCurrentRoom] = normalRoom;
                        map.Add(new Room(xCurrentRoom, yCurrentRoom, normalRoom, left, right, bottom, top));
                        currentMapValue++;
                    }
                    //If the current room is the destination, just add the new door
                    else
                    {
                        Room room = map.Find(r => (r.X == xCurrentRoom) && (r.Y == yCurrentRoom));
                        room.HasDoorBottom = top;
                        room.HasDoorTop = bottom;
                        room.HasDoorLeft = right;
                        room.HasDoorRight = left;
                    }
                }
                else
                {
                    //If I cross another special room than the one intended => Reset and create a new one
                    if ((xCurrentRoom != xRoom || yCurrentRoom != yRoom) && mapLayout[xCurrentRoom, yCurrentRoom] != normalRoom)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        //Creates a number o random rooms
        //Return numbers of room created
        private int CreateRandomRoom(int maxRoomsToCreate)
        {
            int roomCount = 0;
            int randomIndex = 0;
            bool roomCreated = true;
                
            //If the room coudn't be created, return
            //If the number of rooms is reached, return
            //After that, if it's the first room i'm creating, i try continue, if not, there is a % of chance of creating a new room from the created one or to just return
            while ((roomCount == 0 || isProbabilityReached(randomCorridorPourcentage, 100)) && roomCount < maxRoomsToCreate && roomCreated == true)
            {
                //If it's the first room, take a random normal room form the map
                if (roomCount == 0)
                { 
                    do
                    {
                        randomIndex = getRandom(map.Count - 1);
                    } while (map[randomIndex].RoomId != normalRoom);
                }

                //Pick a random direction (number from 0 to 3)

                int randomDirection;
                int tryCount = 0;
                randomDirection = getRandom(3);
                roomCreated = false;

                do
                {
                    try
                    {
                        //In a direction, look if the creation is possible (outofbound or already a room)
                        //If not, try the next direction until done all 4 which end up in a return
                        //The try catch is if i try to reach an outofbound index
                        //If I can create the room, I create it with the good doors and put the good doors to the other connected room
                        //Also put the next index of the room to itself to continue the chain if needed for next loop
                        switch (randomDirection % 4) 
                        {
                            case 0: //Top
                                if (mapLayout[map[randomIndex].X - 1, map[randomIndex].Y ] == 0)
                                {
                                    Room newRoom = new Room(map[randomIndex].X - 1, map[randomIndex].Y, normalRoom, false, false, false, true);
                                    map.Add(newRoom);
                                    mapLayout[map[randomIndex].X - 1, map[randomIndex].Y] = normalRoom;

                                    Room room = map.Find(r => (r.X == map[randomIndex].X) && (r.Y == map[randomIndex].Y));
                                    room.HasDoorTop = true;

                                    randomIndex = map.IndexOf(newRoom);
                                    roomCreated = true;
                                }
                                else
                                {
                                    tryCount++;
                                    randomDirection++;
                                }
                                break;
                            case 1: //Bottom
                                if (mapLayout[map[randomIndex].X + 1, map[randomIndex].Y] == 0)
                                {
                                    Room newRoom = new Room(map[randomIndex].X + 1, map[randomIndex].Y, normalRoom, false, false, true, false);
                                    map.Add(newRoom);
                                    mapLayout[map[randomIndex].X + 1, map[randomIndex].Y ] = normalRoom;

                                    Room room = map.Find(r => (r.X == map[randomIndex].X) && (r.Y == map[randomIndex].Y));
                                    room.HasDoorBottom = true;

                                    randomIndex = map.IndexOf(newRoom);
                                    roomCreated = true;
                                }
                                else
                                {
                                    tryCount++;
                                    randomDirection++;
                                }
                                break;
                            case 2: //Right
                                if (mapLayout[map[randomIndex].X , map[randomIndex].Y + 1] == 0)
                                {
                                    Room newRoom = new Room(map[randomIndex].X, map[randomIndex].Y + 1, normalRoom, false, true, false, false);
                                    map.Add(newRoom);
                                    mapLayout[map[randomIndex].X , map[randomIndex].Y + 1] = normalRoom;

                                    Room room = map.Find(r => (r.X == map[randomIndex].X) && (r.Y == map[randomIndex].Y));
                                    room.HasDoorRight = true;

                                    randomIndex = map.IndexOf(newRoom);
                                    roomCreated = true;
                                }
                                else
                                {
                                    tryCount++;
                                    randomDirection++;
                                }
                                break;
                            case 3: //Left
                                if (mapLayout[map[randomIndex].X , map[randomIndex].Y - 1] == 0)
                                {
                                    Room newRoom = new Room(map[randomIndex].X, map[randomIndex].Y - 1, normalRoom, true, false, false, false);
                                    map.Add(newRoom);
                                    mapLayout[map[randomIndex].X , map[randomIndex].Y - 1] = normalRoom;

                                    Room room = map.Find(r => (r.X == map[randomIndex].X) && (r.Y == map[randomIndex].Y));
                                    room.HasDoorLeft = true;

                                    randomIndex = map.IndexOf(newRoom);
                                    roomCreated = true;
                                }
                                else
                                {
                                    tryCount++;
                                    randomDirection++;
                                }
                                break;
                        }
                    }
                    catch (IndexOutOfRangeException ex)
                    { tryCount++; }
                } while (!roomCreated && tryCount <= 3);

                if (roomCreated)
                    roomCount++;
            }

            return roomCount;
        }

        //Checks a number of rooms in a X,X square and returns if it"s ok or not
        private bool VerifyRoomDisposition(int squareSize, int maxRoomInSquare)
        {
            int offset = (int)squareSize / 2;
            for(int i= offset; i< mapLayout.GetLength(0) - offset ;i++)
            {
                for (int j = offset; j < mapLayout.GetLength(1) - offset ; j++)
                {
                    int numberOfRoomInSquare = 0;
                    for (int k = i-offset; k <= i+offset; k++)
                    {
                        for (int l = j-offset; l <= j+offset; l++)
                        {
                            if (mapLayout[k, l] != emptyRoom)
                                numberOfRoomInSquare++;
                                
                            if (numberOfRoomInSquare > maxRoomInSquare)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        //Checks a number of rooms in lines and returns if the amount is under the max defined or not
        private bool VerifyCoridorDisposition(int maxRoomInCoridor)
        {
            for (int i = 0; i < mapLayout.GetLength(0); i++)
            {
                int numberOfRoomInXCorridor = 0;
                for (int j = 0; j < mapLayout.GetLength(1); j++)
                {
                    if (mapLayout[i, j] != emptyRoom)
                        numberOfRoomInXCorridor++;
                    else
                        numberOfRoomInXCorridor = 0;

                    if (numberOfRoomInXCorridor > maxRoomInCoridor)
                        return false;
                }
            }

            for (int j = 0; j < mapLayout.GetLength(0); j++)
            {
                int numberOfRoomInXCorridor = 0;
                for (int i = 0; i < mapLayout.GetLength(1); i++)
                {
                    if (mapLayout[i, j] != emptyRoom)
                        numberOfRoomInXCorridor++;
                    else
                        numberOfRoomInXCorridor = 0;

                    if (numberOfRoomInXCorridor > maxRoomInCoridor)
                        return false;
                }
            }

            return true;
        }

        //Gets a random number between 0 and maxvalue 
        private int getRandom(int maxValue)
        {
            Random random = new Random();
            maxValue++;
            return ((int)(random.NextDouble() * 100 * maxValue)) % maxValue;
        }

        //Returns if the probability has been eached or not
        private bool isProbabilityReached(float chances, int maxChanceValue)
        {
            Random randomObj = new Random();
            int random = (int)(randomObj.NextDouble() * maxChanceValue);

            if (random <= chances)
                return true;
            else
                return false;
        }

        private void ShowMapTerminal()
        {
            Console.WriteLine("Map with no doors");

            for (int i = 0; i < mapLayout.GetLength(0); i++)
            {
                for (int j = 0; j < mapLayout.GetLength(1); j++)
                {
                    switch (mapLayout[i,j])
                    {
                        case 0:
                            Console.Write("  ");
                            break;
                        case 1:
                            Console.Write("S ");
                            break;
                        case 2:
                            Console.Write("H ");
                            break;
                        case 3:
                            Console.Write("W ");
                            break;
                        case 4:
                            Console.Write("B ");
                            break;
                        case 5:
                            Console.Write("O ");
                            break;
                    }                    
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Map with doors");

            char[,] charMap = new char[mapSize*2, mapSize * 2];

            for (int i = 0; i < charMap.GetLength(0); i++)
            {
                for (int j = 0; j < charMap.GetLength(1); j++)
                {
                    charMap[i, j] = ' ';
                }
            }

            foreach (Room room in map)
            {
                switch (room.RoomId)
                {
                    case 0:
                        charMap[room.X*2 ,room.Y * 2 ] = ' ';
                        break;
                    case 1:
                        charMap[room.X * 2 , room.Y * 2 ] = 'S';
                        break;
                    case 2:
                        charMap[room.X * 2, room.Y * 2] = 'H';
                        break;
                    case 3:
                        charMap[room.X * 2 , room.Y * 2 ] = 'W';
                        break;
                    case 4:
                        charMap[room.X * 2 , room.Y * 2 ] = 'B';
                        break;
                    case 5:
                        charMap[room.X * 2 , room.Y * 2 ] = 'O';
                        break;
                }

                if(room.HasDoorBottom)
                    charMap[room.X * 2 +1 , (room.Y * 2) ] = '|';

                if (room.HasDoorTop)
                    charMap[room.X * 2 -1 , (room.Y * 2 )] = '|';

                if (room.HasDoorLeft)
                    charMap[(room.X * 2 ) , room.Y * 2 -1] = '-';

                if (room.HasDoorRight)
                    charMap[(room.X * 2 ) , room.Y * 2 +1] = '-';
            }

            for (int i = 0; i < charMap.GetLength(0); i++)
            {
                for (int j = 0; j < charMap.GetLength(1); j++)
                {
                    Console.Write(charMap[i,j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

    }

    public class Room
    {
        private bool _hasDoorRight;
        private bool _hasDoorLeft;
        private bool _hasDoorTop;
        private bool _hasDoorBottom;

        private int _roomId;

        private int _x;
        private int _y;

        public Room(int x, int y , int roomId ,bool hasDoorRight, bool hasDoorLeft, bool hasDoorTop, bool hasDoorBottom)
        {
            HasDoorRight = hasDoorRight;
            HasDoorLeft = hasDoorLeft;
            HasDoorTop = hasDoorTop;
            HasDoorBottom = hasDoorBottom;
            RoomId = roomId;
            X = x;
            Y = y;
        }

        public Room(int x, int y, int roomId)
        {
            RoomId = roomId;
            X = x;
            Y = y;
        }

        public bool HasDoorRight { get => _hasDoorRight; set => _hasDoorRight = value; }
        public bool HasDoorLeft { get => _hasDoorLeft; set => _hasDoorLeft = value; }
        public bool HasDoorTop { get => _hasDoorTop; set => _hasDoorTop = value; }
        public bool HasDoorBottom { get => _hasDoorBottom; set => _hasDoorBottom = value; }
        public int RoomId { get => _roomId; set => _roomId = value; }
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
    }

}
