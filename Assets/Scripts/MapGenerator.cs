using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int test;

    public GameObject[] block = new GameObject[15];
    float[,] map;
    float[,] shapeMap;
    public int sizeX = 100;
    public int sizeY = 100;
    public int distanceX = 0;
    public int distanceY = 0;
    int modifiedSizeX = 0;
    int modifiedSizeY = 0;

    public float fillPercent;
    public int resRefinement;
    public int preReselutionRefinement;

    public int postReselutionRefinement;
    public float scale = 0.1f;
    [SerializeField]
    public int? seed = 0;
    public List<rockFormation> rockFormations = new List<rockFormation>();

    //split every square to 9 pieces
    //Either have a chance of cutting of corners or run refine, maybe both?


    // Start is called before the first frame update
    void Start()
    {
        //move stuff apart instead of doing the wierd pixel increase thing
        //do it with paint bucket to decide where the stuff is, then move apart
        //find bottom left corner to know how much to increase the map size with. 
        //Then move n times to the right/down. n is in what order they are in. 
        //the one one the left goes 0 times, then one for the right of that one go 1, the next 2 and so on.
        calculateMap();
        moveRockFormations(ref map);
        calculateShape(map);
        

        for (int x = 0; x < modifiedSizeX - 1; x++)
        {
            for (int y = 0; y < modifiedSizeY- 1; y++)
            {
                try
                {
                    if (shapeMap[x, y] != 0)
                    {
                        Instantiate(block[(int)shapeMap[x, y] - 1], new Vector3(x * scale, y * scale - modifiedSizeY * scale), this.transform.rotation);
                    }
                }
                catch (System.Exception)
                {
                    Instantiate(block[14], new Vector3(x * scale, y * scale), this.transform.rotation);
                    Debug.Log("Missing texture");
                }

            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void calculateShape(float[,] map)
    {
        shapeMap = new float[modifiedSizeX - 1, modifiedSizeY - 1];

        int[] num = { 1, 2, 4, 8 };
        for (int x = 0; x < modifiedSizeX - 1; x++)
        {
            for (int y = 0; y < modifiedSizeY - 1; y++)
            {
                for (int x2 = 0; x2 < 2; x2++)
                {
                    for (int y2 = 0; y2 < 2; y2++)
                    {
                        shapeMap[x, y] += map[x + x2, y + y2] * num[x2 * 2 + y2];
                    }
                }
            }
        }
    }

    void calculateMap()
    {
        modifiedSizeX = sizeX;
        modifiedSizeY = sizeY;
        if (seed != null)
        {
            Random.InitState((int)seed);
        }

        map = new float[modifiedSizeX, modifiedSizeY];

        for (int x = 0; x < modifiedSizeX; x++)
        {
            for (int y = 0; y < modifiedSizeY; y++)
            {
                if (Random.Range(0, 99) < fillPercent)
                {
                    map[x, y] = 1;
                }
                else map[x, y] = 0;
            }
        }

        for (int i = 0; i < preReselutionRefinement; i++)
        {
            refineCave(ref map);
        }

        increaseReselution(ref map, resRefinement);
        
        for (int i = 0; i < postReselutionRefinement; i++)
        {
            refineCave(ref map);
        }
    }

    void increaseReselution(ref float[,] map, int increase)
    {
        float[,] tempMap = new float[modifiedSizeX * increase, modifiedSizeY * increase];

        for (int x = 0; x < modifiedSizeX; x++)
        {
            for (int y = 0; y < modifiedSizeY; y++)
            {
                for (int x2 = 0; x2 < increase; x2++)
                {
                    for (int y2 = 0; y2 < increase; y2++)
                    {
                        tempMap[x*increase + x2, y*increase + y2] = map[x, y];
                    }
                }
            }
        }

        modifiedSizeX *= increase;
        modifiedSizeY *= increase;

        map = tempMap;
    }

    void refineCave(ref float[,] map)
    {
        float[,] tempMap = new float[modifiedSizeX, modifiedSizeY];

        for (int x = 0; x < modifiedSizeX - 2; x++)
        {
            for (int y = 0; y < modifiedSizeY - 2; y++)
            {
                int k = 0;
                for (int x2 = 0; x2 < 3; x2++)
                {
                    for (int y2 = 0; y2 < 3; y2++)
                    {
                        if(map[x + x2, y + y2] == 1 && (x2 != 1 || y2 != 1))
                        {
                            k += 1;
                        }
                    }
                }
                if(k < 5)
                {
                    tempMap[x+1, y + 1] = 0;
                }
                else if(k > 5)
                {
                    tempMap[x + 1, y + 1] = 1;
                }
            }
        }
        //add edge and corners!
        map = tempMap;
    }

    void moveRockFormations(ref float[,] map)
    {
        rockFormations = new List<rockFormation>();
        //Add the variable that let you scale the thingy
        //The rocks that has been used to generate rock formations
        int[,] scannedRocks = new int[modifiedSizeX,modifiedSizeY];

        List<List<rockFormation>> orderX = new List<List<rockFormation>>();
        List<List<rockFormation>> orderY = new List<List<rockFormation>>();

        for (int x = 0; x < modifiedSizeX; x++)
        {
            orderX.Add(new List<rockFormation>());
        }
        for (int y = 0; y < modifiedSizeY; y++)
        {
            orderY.Add(new List<rockFormation>());
        }

        for (int x = 0; x < modifiedSizeX; x++)
        {
            for (int y = 0; y < modifiedSizeY; y++)
            {
                if (map[x, y] == 1 && scannedRocks[x, y] == 0)
                {

                    List<Vector2> inQueue = new List<Vector2>();
                    rockFormation rock = new rockFormation(new Vector2(x, y));

                    orderX[x].Add(rock);
                    orderY[y].Add(rock);

                    inQueue.Add(rock.mainPoint);

                    while(inQueue.Count > 0)
                    {
                        if (inQueue[0].x - 1 >= 0)
                        {
                            if(scannedRocks[(int)inQueue[0].x - 1, (int)inQueue[0].y] == 0)
                            {
                                scannedRocks[(int)inQueue[0].x - 1, (int)inQueue[0].y] = 1;
                                if (map[(int)inQueue[0].x - 1, (int)inQueue[0].y] == 1)
                                {
                                    rock.points.Add(new Vector2((int)inQueue[0].x - 1 - rock.mainPoint.x, (int)inQueue[0].y -rock.mainPoint.y));
                                    inQueue.Add(new Vector2((int)inQueue[0].x - 1, (int)inQueue[0].y));

                                    if (rock.highestPos.x < rock.points[rock.points.Count - 1].x)
                                    {
                                        rock.highestPos.x = (int)rock.points[rock.points.Count - 1].x;
                                    }
                                    if (rock.highestPos.y < rock.points[rock.points.Count - 1].y)
                                    {
                                        rock.highestPos.y = (int)rock.points[rock.points.Count - 1].y;
                                    }
                                }

                            }
                        }
                        if (inQueue[0].x + 1 < modifiedSizeX)
                        {
                            if (scannedRocks[(int)inQueue[0].x + 1, (int)inQueue[0].y] == 0)
                            {
                                scannedRocks[(int)inQueue[0].x + 1, (int)inQueue[0].y] = 1;
                                if (map[(int)inQueue[0].x + 1, (int)inQueue[0].y] == 1)
                                {
                                    rock.points.Add(new Vector2((int)inQueue[0].x + 1 - rock.mainPoint.x, (int)inQueue[0].y - rock.mainPoint.y));
                                    inQueue.Add(new Vector2((int)inQueue[0].x + 1, (int)inQueue[0].y));

                                    if (rock.highestPos.x < rock.points[rock.points.Count-1].x)
                                    {
                                        rock.highestPos.x = (int)rock.points[rock.points.Count - 1].x;
                                    }
                                    if (rock.highestPos.y < rock.points[rock.points.Count - 1].y)
                                    {
                                        rock.highestPos.y = (int)rock.points[rock.points.Count - 1].y;
                                    }

                                }

                            }
                        }
                        if (inQueue[0].y - 1 >= 0)
                        {
                            if(scannedRocks[(int)inQueue[0].x, (int)inQueue[0].y - 1]  == 0)
                            {
                                scannedRocks[(int)inQueue[0].x, (int)inQueue[0].y - 1] = 1;
                                if(map[(int)inQueue[0].x, (int)inQueue[0].y - 1] == 1)
                                {
                                    rock.points.Add(new Vector2((int)inQueue[0].x - rock.mainPoint.x, (int)inQueue[0].y -1 - rock.mainPoint.y));
                                    inQueue.Add(new Vector2((int)inQueue[0].x, (int)inQueue[0].y-1));

                                    if (rock.highestPos.x < rock.points[rock.points.Count - 1].x)
                                    {
                                        rock.highestPos.x = (int)rock.points[rock.points.Count - 1].x;
                                    }
                                    if (rock.highestPos.y < rock.points[rock.points.Count - 1].y)
                                    {
                                        rock.highestPos.y = (int)rock.points[rock.points.Count - 1].y;
                                    }
                                }
                            }
                        }
                        if (inQueue[0].y + 1 < modifiedSizeY)
                        {
                            if (scannedRocks[(int)inQueue[0].x, (int)inQueue[0].y + 1] == 0)
                            {
                                scannedRocks[(int)inQueue[0].x, (int)inQueue[0].y + 1] = 1;
                                if (map[(int)inQueue[0].x, (int)inQueue[0].y + 1] == 1)
                                {
                                    rock.points.Add(new Vector2((int)inQueue[0].x - rock.mainPoint.x, (int)inQueue[0].y + 1 - rock.mainPoint.y));
                                    inQueue.Add(new Vector2((int)inQueue[0].x, (int)inQueue[0].y + 1));

                                    if (rock.highestPos.x < rock.points[rock.points.Count - 1].x)
                                    {
                                        rock.highestPos.x = (int)rock.points[rock.points.Count - 1].x;
                                    }
                                    if (rock.highestPos.y < rock.points[rock.points.Count - 1].y)
                                    {
                                        rock.highestPos.y = (int)rock.points[rock.points.Count - 1].y;
                                    }
                                }
                            }
                        }
                        inQueue.RemoveAt(0);
                    }
                    rockFormations.Add(rock);
                }
                
                scannedRocks[x, y] = 1;
            }
        }

        for (int x = modifiedSizeX - 1; x >= 0; x--)
        {
            if (orderX[x].Count == 0)
            {
                orderX.RemoveAt(x);
            }
        }
        for (int y = modifiedSizeY - 1; y >= 0; y--)
        {
            if (orderY[y].Count == 0)
            {
                orderY.RemoveAt(y);
            }
        }

        int highestX = 0;
        int highestY = 0;


        for (int x = 0; x < orderX.Count; x++)
        {
            for (int i = 0; i < orderX[x].Count; i++)
            {
                orderX[x][i].offset.x = x * distanceX;
                if (highestX < orderX[x][i].highestPos.x + orderX[x][i].mainPoint.x + orderX[x][i].offset.x)
                {
                    highestX = (int)(orderX[x][i].highestPos.x + orderX[x][i].mainPoint.x + orderX[x][i].offset.x);
                }
            }
        }

        for (int y = 0; y < orderY.Count; y++)
        {
            for (int i = 0; i < orderY[y].Count; i++)
            {
                orderY[y][i].offset.y = y * distanceY;
                if (highestY < orderY[y][i].highestPos.y + orderY[y][i].mainPoint.y + orderY[y][i].offset.y)
                {
                    highestY = (int)(orderY[y][i].highestPos.y + orderY[y][i].mainPoint.y + orderY[y][i].offset.y);
                }
            }
        }

        modifiedSizeX = highestX + 1;
        modifiedSizeY = highestY + 1;

        float[,] tempMap = new float[modifiedSizeX,modifiedSizeY];

        for (int i = 0; i < rockFormations.Count; i++)
        {
            
            tempMap[(int)(rockFormations[i].mainPoint.x + rockFormations[i].offset.x),
                    (int)(rockFormations[i].mainPoint.y + rockFormations[i].offset.y)] = 1;
            for (int p = 0; p < rockFormations[i].points.Count; p++)
            {
                try
                {
                    tempMap[(int)(rockFormations[i].mainPoint.x + rockFormations[i].points[p].x
                    + rockFormations[i].offset.x),
                    (int)(rockFormations[i].mainPoint.y + rockFormations[i].points[p].y
                    + rockFormations[i].offset.y)] = 1;
                }
                catch (System.Exception)
                {
                    Debug.Log((int)(rockFormations[i].mainPoint.x + rockFormations[i].points[p].x
                    + rockFormations[i].offset.x) + ", " +
                            (int)(rockFormations[i].mainPoint.y + rockFormations[i].points[p].y
                    + rockFormations[i].offset.y) + " - " + highestX + ", " + highestY);
                    throw;
                }
            }
        }
        map = tempMap;
    }


    void OnDrawGizmos()
    {
        calculateMap();
        moveRockFormations(ref map);
        calculateShape(map);
        
        Gizmos.color = new Color(1f, 0.5f, 0, 1);
        if (test != 0)
        {
            Gizmos.DrawWireCube(-rockFormations[test - 1].mainPoint*scale, new Vector3(1, 1, 1) * scale * 0.8f);
            for (int i = 0; i < rockFormations[test - 1].points.Count; i++)
            {
                Gizmos.DrawWireCube(- (rockFormations[test - 1].points[i] + rockFormations[test - 1].mainPoint) * scale, new Vector3(1, 1, 1) * scale * 0.8f);
            }
        }
        else
        {
            //add -1 on increased scale and change to map to shapemap
            for (int x = 0; x < modifiedSizeX-1; x++)
            {
                for (int y = 0; y < modifiedSizeY-1; y++)
                {
                    if (shapeMap[x, y] != 0)
                    {
                        Gizmos.DrawWireCube(new Vector3(x * scale, y * scale - modifiedSizeY * scale), new Vector3(1, 1, 1) * scale * 0.8f);
                    }
                }
            }
        }
    }


}

public class rockFormation
{
    public Vector2 mainPoint;
    public List<Vector2> points = new List<Vector2>();

    public Vector2 highestPos = new Vector2(0, 0);
    public Vector2 offset = new Vector2(0, 0);

    public rockFormation(Vector2 MainPoint)
    {
        mainPoint = MainPoint;
    }
}
