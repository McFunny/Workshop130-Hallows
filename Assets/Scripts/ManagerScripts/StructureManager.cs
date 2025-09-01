using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class StructureManager : MonoBehaviour
{
    public static StructureManager Instance;
    [Header("Tiles")]
    public Tilemap farmTileMap, cabinTileMap, barnTileMap;
    public TileBase freeTile, occupiedTile, borderTile; //border tiles cannot be changed nor interacted with the player, but enemies could use them 
    //Would also then need an occupied border tile

    public List<StructureBehaviorScript> allStructs; //MUST BE SAVED

    public GameObject weedTile, farmTree, farmTile, crowPod, crowWithNut, boulder, buriedItem, barricade, trough, wBearTrap, bearTrap, critterHive;
    public CropData fogChime, berryBush;

    //Game will compare the two to find out which tile position correlates with the nutrients associated with it.
    List<Vector3Int> allFarmTiles = new List<Vector3Int>();
    List<Vector3Int> allBarnTiles = new List<Vector3Int>();
    List<NutrientStorage> storage = new List<NutrientStorage>(); //MUST BE SAVED

    public List<NutrientStorage> Storage => storage;

    [Header("Debugs")]
    public bool ignoreCropGrowthTime = false; //if true, each growth phase takes an hour
    public bool enableCheats = false;
    public bool forceSurvivalMode = false;
    public bool forceSellSiegeSeeds = false; //If true, the apoth will have the bools ticked as if she has already seen the scroll
    public bool disableBarricades = false; //If true, all fallen trees will already be cleared


    void Awake()
    {
        if(forceSurvivalMode) MainMenuScript.currentFileMode = FileMode.Survival;
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            print("Destroyed Copy");
            return;
        }
        else
        {
            Instance = this;
        }
        InstantiateNutrientStorage();
        //load in all the saved data, such as the nutrient storages and alltiles list. If Main Menu doesnt start a new game, then dont populate this stuff below
        if(!MainMenuScript.loadingData)
        {
            StartCoroutine(SpawnStartingStructures()); //Only do this when a new game has started.
        }
        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void Start()
    {
        PopulateForageables(1, 4);
        PopulateDecorCrows(0, 2);
        StartCoroutine(PopulateStructure(-2, 3, buriedItem, true, farmTileMap));

        if(forceSellSiegeSeeds)
        {
            GameSaveData.Instance.apo_readScroll = true;
        }
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
        if(Instance != null && Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadNutrients(NutrientStorage[] newStorage)
    {
        storage.Clear();
        storage = newStorage.ToList();
        for(int i = 0; i < storage.Count; i++)
        {
            if(storage[i].waterLevel > 3) print("Water!!!???");
        }
    }

    public void HourUpdate()
    {
        //print("AllStructs: " + allStructs.Count);
        if(TimeManager.Instance.currentHour == 8)
        {
            StartCoroutine(PopulateStructure(-3, 5, weedTile, false, farmTileMap));
            PopulateDecorCrows(0, 2);
            StartCoroutine(PopulateStructure(-2, 3, boulder, true, farmTileMap));
            PopulateBerryBushes(-5, 2, false);
        }
        if(TimeManager.Instance.currentHour == 6)
        {
            PopulateForageables(-1, 3);
        }
        if(TimeManager.Instance.currentHour == 20 && !NightSpawningManager.Instance.boxPlaced) PopulateNightWeeds(1, 6);

        if(Random.Range(0,100) < 7)
        {
            Instantiate(crowWithNut, NightSpawningManager.Instance.RandomMistPosition(), Quaternion.identity);
        }
    }

    /*[ContextMenu("NutCrowTest")]
    public void CrowTest()
    {
        Instantiate(crowWithNut, NightSpawningManager.Instance.RandomMistPosition(), Quaternion.identity);
    }*/

    public void GameOver()
    {
        if(TimeManager.Instance.isDay) return;
        float r;
        int s = 0;
        for(int i = 0; i < allStructs.Count; i++)
        {
            if(allStructs[i] && allStructs[i].destructable)
            {
                FarmLand potentialWeed = allStructs[i] as FarmLand;
                if(potentialWeed && potentialWeed.isWeed) continue;

                r = Random.Range(0, 10);
                if(potentialWeed) r += 2;
                if(MainMenuScript.currentFileMode == FileMode.Cozy) r -= 2;
                if((r >= 6.5f || allStructs[i].onFire) && !allStructs[i].absentFromFarmGrid) //Destroy structure.
                {
                    print("Deleting: " + allStructs[i]);
                    //Destroy(allStructs[i].gameObject);
                    allStructs[i].TakeDamage(999);
                    s++;
                }
            }
        }
        print(s + " Structures were deleted");
    }

#region TileCommands

    public Tilemap CurrentTileMap(Vector3 pos)
    {
        Vector3Int gridPos;

        if(farmTileMap)
        {
            gridPos = farmTileMap.WorldToCell(pos);
            if(farmTileMap.GetTile(gridPos) != null)
            {
                //print("Tile is on farm grid");
                return farmTileMap;
            }
        }

        if(cabinTileMap)
        {
            gridPos = cabinTileMap.WorldToCell(pos);
            if(cabinTileMap.GetTile(gridPos) != null)
            {
                //print("Tile is on cabin grid");
                return cabinTileMap;
            }
        }

        if(barnTileMap)
        {
            gridPos = barnTileMap.WorldToCell(pos);
            if(barnTileMap.GetTile(gridPos) != null)
            {
                //print("Tile is on town grid");
                return barnTileMap;
            }
        }
        //print("No tile grid was found");
        return null;
    }

    public bool ValidateGridType(Vector3 pos, List<GridType> types)
    {
        foreach(GridType g in types)
        {
            switch(g)
            {
                case GridType.Any:
                    if(CurrentTileMap(pos) != null) return true;
                    break;
                case GridType.Farm:
                    if(CurrentTileMap(pos) == farmTileMap) return true;
                    break;
                case GridType.Cabin:
                    if(CurrentTileMap(pos) == cabinTileMap) return true;
                    break;
                case GridType.Barn:
                    if(CurrentTileMap(pos) == barnTileMap) return true;
                    break;
            }
        }
        
        return false;
    }

    public bool ValidateGridType(Vector3 pos, GridType type)
    {
        switch(type)
        {
            case GridType.Any:
                if(CurrentTileMap(pos) != null) return true;
                break;
            case GridType.Farm:
                if(CurrentTileMap(pos) == farmTileMap) return true;
                else return false;
                break;
            case GridType.Cabin:
                if(CurrentTileMap(pos) == cabinTileMap) return true;
                else return false;
                break;
            case GridType.Barn:
                if(CurrentTileMap(pos) == barnTileMap) return true;
                else return false;
                break;
        }
        return false;
    }

    public Vector3 CheckTile(Vector3 pos) //1x1
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return new Vector3 (0,0,0);

        //Grab tile position
        Vector3Int gridPos = currentMap.WorldToCell(pos);

        TileBase currentTile = currentMap.GetTile(gridPos);

        //Is the tile on the grid and open?
        if(currentTile != null && currentTile == freeTile)
        {
            Vector3 spawnPos = currentMap.GetCellCenterWorld(gridPos); //Return the position of the open tile
            return spawnPos;
        } 
        else return new Vector3 (0,0,0); //Will not spawn
    }

    public Vector3 CheckLargeTile(Vector3 pos) //2x2
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return new Vector3 (0,0,0);

        List<Vector3Int> selectedTiles = new List<Vector3Int>();
        Vector3Int gridPos = currentMap.WorldToCell(pos);
        selectedTiles.Add(gridPos); //Top Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y)); //Top Right Tile
        selectedTiles.Add(new Vector3Int(gridPos.x, gridPos.y - 1)); //Bottom Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y - 1)); //Bottom Right Tile

        foreach(Vector3Int _pos in selectedTiles)
        {
            TileBase currentTile = currentMap.GetTile(_pos); //Is the tile free?
            if(currentTile == null || currentTile != freeTile) return new Vector3 (0,0,0);
        }

        Vector3 start = currentMap.GetCellCenterWorld(gridPos);
        Vector3 otherEnd = currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y - 1));
        Vector3 center = new Vector3((start.x + otherEnd.x)/2, (start.y + otherEnd.y)/2, (start.z + otherEnd.z)/2); 
        //The center of the 2x2 Square
        
        return center;
    }

    public Vector3 CheckExtraLargeTile(Vector3 pos) //3x3
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return new Vector3 (0,0,0);

        List<Vector3Int> selectedTiles = new List<Vector3Int>();
        Vector3Int gridPos = currentMap.WorldToCell(pos);
        selectedTiles.Add(gridPos); //Middle Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y + 1)); //Top Right Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y)); //Right Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y - 1)); //Bottom Right Tile
        selectedTiles.Add(new Vector3Int(gridPos.x, gridPos.y + 1)); //Top Middle Tile
        selectedTiles.Add(new Vector3Int(gridPos.x, gridPos.y - 1)); //Bottom Middle Tile
        selectedTiles.Add(new Vector3Int(gridPos.x - 1, gridPos.y - 1)); //Bottom Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x - 1, gridPos.y)); //Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x - 1, gridPos.y + 1)); //Top Left Tile

        foreach(Vector3Int _pos in selectedTiles)
        {
            TileBase currentTile = currentMap.GetTile(_pos); //Is the tile free?
            if(currentTile == null || currentTile != freeTile) return new Vector3 (0,0,0);
        }

        Vector3 spawnPos = currentMap.GetCellCenterWorld(gridPos); //Return the position of the open tile
        return spawnPos;
    }

    public Vector3 CheckOneByTwoTile(Vector3 pos, Quaternion rot) //1x2
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return new Vector3 (0,0,0);

        List<Vector3Int> selectedTiles = new List<Vector3Int>();
        Vector3Int gridPos = currentMap.WorldToCell(pos);

        selectedTiles.Add(gridPos);
        if(rot.y == 0|| rot.y == -1 || rot.y == 1) //item is sideways //problem is here
        {
            selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y)); //Top Right Tile
            print("Sideways");
        }
        else
        {
            selectedTiles.Add(new Vector3Int(gridPos.x, gridPos.y - 1)); //Bottom Left Tile
            print("Straight");
        }
        print(rot.y);

        foreach(Vector3Int _pos in selectedTiles)
        {
            TileBase currentTile = currentMap.GetTile(_pos); //Is the tile free?
            if(currentTile == null || currentTile != freeTile) return new Vector3 (0,0,0);
        }

        Vector3 start = currentMap.GetCellCenterWorld(gridPos);
        Vector3 otherEnd = currentMap.GetCellCenterWorld(selectedTiles[1]);
        Vector3 center = new Vector3((start.x + otherEnd.x)/2, (start.y + otherEnd.y)/2, (start.z + otherEnd.z)/2); 
        //The center of the 1x2 Square
        
        return center;
    }

    public Vector3 GetTileCenter(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return new Vector3 (0,0,0);

        //Grab tile position
        Vector3Int gridPos = currentMap.WorldToCell(pos);

        if(currentMap.GetTile(gridPos) != null && currentMap.GetTile(gridPos) != borderTile) return currentMap.GetCellCenterWorld(gridPos);
        else return new Vector3 (0,0,0);
    }

    public Vector3 GetRandomTile()
    {
        //currently for farm tiles only
        if(allFarmTiles.Count == 0)
        {
            print("No available tiles");
            return new Vector3 (0,0,0);
        }
        int r = Random.Range(0, allFarmTiles.Count);
        return farmTileMap.GetCellCenterWorld(allFarmTiles[r]);
    }

    public Vector3 GetRandomTile(GridType type)
    {
        switch(type)
        {
            case GridType.Barn:
                if(allBarnTiles.Count == 0)
                {
                    print("No available tiles");
                    return new Vector3 (0,0,0);
                }
                int r = Random.Range(0, allBarnTiles.Count);
                return barnTileMap.GetCellCenterWorld(allBarnTiles[r]);
                break;
            default :
                return GetRandomTile();
                break;
        }
    }

    public Vector3 GetRandomNearbyTile(GridType type, float range, Vector3 pos)
    {
        int i = 0;
        switch(type)
        {
            case GridType.Barn:
                if(allBarnTiles.Count == 0)
                {
                    print("No available tiles");
                    return new Vector3 (0,0,0);
                }
                while(i < 30)
                {
                    int r = Random.Range(0, allBarnTiles.Count);
                    if(Vector3.Distance(barnTileMap.GetCellCenterWorld(allBarnTiles[r]), pos) <= range) return barnTileMap.GetCellCenterWorld(allBarnTiles[r]);
                    i++;
                }
                print("No nearby tiles");
                return new Vector3 (0,0,0);
                break;
            default :
                if(allFarmTiles.Count == 0)
                {
                    print("No available tiles");
                    return new Vector3 (0,0,0);
                }
                while(i < 30)
                {
                    int r = Random.Range(0, allFarmTiles.Count);
                    if(Vector3.Distance(farmTileMap.GetCellCenterWorld(allFarmTiles[r]), pos) <= range) return farmTileMap.GetCellCenterWorld(allFarmTiles[r]);
                    i++;
                }
                print("No nearby tiles");
                return new Vector3 (0,0,0);
                break;
        }
    }

    public Vector3 GetRandomClearTile()
    {
        //currently for farm tiles only
        Vector3 tilePos = new Vector3 (0,0,0);
        int t = 0;
        do
        {
            int r = Random.Range(0, allFarmTiles.Count);
            TileBase currentTile = farmTileMap.GetTile(allFarmTiles[r]);
            if(currentTile != null && currentTile == freeTile) tilePos = farmTileMap.GetCellCenterWorld(allFarmTiles[r]);
            t++;
        }
        while(t < 15 && tilePos == new Vector3 (0,0,0));
        return tilePos;
    }

    public List<Vector3> GetAdjacentClearTiles(Vector3 pos)
    {
        List<Vector3> adjacentTiles = new List<Vector3>();

        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return adjacentTiles;

        Vector3Int gridPos = currentMap.WorldToCell(pos);

        if(currentMap.GetTile(new Vector3Int(gridPos.x + 1, gridPos.y)) == freeTile) adjacentTiles.Add(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y)));
        if(currentMap.GetTile(new Vector3Int(gridPos.x - 1, gridPos.y)) == freeTile) adjacentTiles.Add(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x - 1, gridPos.y)));
        if(currentMap.GetTile(new Vector3Int(gridPos.x, gridPos.y + 1)) == freeTile) adjacentTiles.Add(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y + 1)));
        if(currentMap.GetTile(new Vector3Int(gridPos.x, gridPos.y - 1)) == freeTile) adjacentTiles.Add(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y - 1)));
        return adjacentTiles;
    }

    public void SpawnStructure(GameObject obj, Vector3 pos)
    {
        Instantiate(obj, pos, Quaternion.identity);
        //SetTile(pos);
    }

    public GameObject SpawnStructureWithInstance(GameObject obj, Vector3 pos)
    {
        GameObject instance = Instantiate(obj, pos, Quaternion.identity);
        //SetTile(pos);
        return instance;
    }

    public bool SpawnLargeStructure(GameObject obj, Vector3 pos, bool randomizeRotation)
    { 
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return false;

        List<Vector3Int> selectedTiles = new List<Vector3Int>();
        Vector3Int gridPos = currentMap.WorldToCell(pos);
        selectedTiles.Add(gridPos); //Top Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y)); //Top Right Tile
        selectedTiles.Add(new Vector3Int(gridPos.x, gridPos.y - 1)); //Bottom Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y - 1)); //Bottom Right Tile

        foreach(Vector3Int _pos in selectedTiles)
        {
            TileBase currentTile = currentMap.GetTile(_pos); //Is the tile free?
            if(currentTile == null || currentTile != freeTile) return false;
        }

        Vector3 start = currentMap.GetCellCenterWorld(gridPos);
        Vector3 otherEnd = currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y - 1));
        Vector3 center = new Vector3((start.x + otherEnd.x)/2, (start.y + otherEnd.y)/2, (start.z + otherEnd.z)/2); 
        //The center of the 2x2 Square
        
        GameObject newObject = Instantiate(obj, center, Quaternion.identity);
        if(randomizeRotation) newObject.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
        return true;
    }

    public bool Spawn1X2Structure(GameObject obj, Vector3 pos)
    { 
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return false;

        List<Vector3Int> selectedTiles = new List<Vector3Int>();
        Vector3Int gridPos = currentMap.WorldToCell(pos);
        selectedTiles.Add(gridPos); //Top Left Tile
        selectedTiles.Add(new Vector3Int(gridPos.x + 1, gridPos.y)); //Top Right Tile

        foreach(Vector3Int _pos in selectedTiles)
        {
            TileBase currentTile = currentMap.GetTile(_pos); //Is the tile free?
            if(currentTile == null || currentTile != freeTile) return false;
        }

        Vector3 start = currentMap.GetCellCenterWorld(gridPos);
        Vector3 otherEnd = currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y));
        Vector3 center = new Vector3((start.x + otherEnd.x)/2, (start.y + otherEnd.y)/2, (start.z + otherEnd.z)/2); 
        //The center of the 2x2 Square
        
        GameObject newObject = Instantiate(obj, center, Quaternion.identity);
        return true;
    }

    public void SetTile(Vector3 pos) 
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;

        Vector3Int gridPos = currentMap.WorldToCell(pos);
        if(currentMap.GetTile(gridPos) == null) return;
        currentMap.SetTile(gridPos, occupiedTile);
    }

    public void SetLargeTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;

        foreach (var gridPosition in currentMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = currentMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= 3f)
            {
                if(currentMap.GetTile(gridPosition) != null) currentMap.SetTile(gridPosition, occupiedTile);
                //print("FoundTile");
            }
        }
    }

    public void SetExtraLargeTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;

        foreach (var gridPosition in currentMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = currentMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= 4.5f)
            {
                if(currentMap.GetTile(gridPosition) != null) currentMap.SetTile(gridPosition, occupiedTile);
                //print("FoundTile");
            }
        }
    }

    public void SetOneByTwoTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;

        foreach (var gridPosition in currentMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = currentMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= 2f)
            {
                if(currentMap.GetTile(gridPosition) != null) currentMap.SetTile(gridPosition, occupiedTile);
                //print("FoundTile");
            }
        }
    }

    public void ClearTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;

        Vector3Int gridPos = currentMap.WorldToCell(pos);
        if(currentMap.GetTile(gridPos) == null) return;
        currentMap.SetTile(gridPos, freeTile);
    }

    public void ClearLargeTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;
        //fetch tiles within a small radius, should return the 4 its occupying
        //print("Clearing");
        foreach (var gridPosition in currentMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = currentMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= 3f)
            {
                if(currentMap.GetTile(gridPosition) != null) currentMap.SetTile(gridPosition, freeTile);
                //print("FoundTile");
            }
            
        }
    }

    public void ClearExtraLargeTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;
        //fetch tiles within a small radius, should return the 4 its occupying
        //print("Clearing");
        foreach (var gridPosition in currentMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = currentMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= 4.5f)
            {
                if(currentMap.GetTile(gridPosition) != null) currentMap.SetTile(gridPosition, freeTile);
                //print("FoundTile");
            }
            
        }
    }

    public void ClearOneByTwoTile(Vector3 pos)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return;
        //fetch tiles within a small radius, should return the 4 its occupying
        //print("Clearing");
        foreach (var gridPosition in currentMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = currentMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= 2f)
            {
                if(currentMap.GetTile(gridPosition) != null) currentMap.SetTile(gridPosition, freeTile);
                //print("FoundTile");
            }
            
        }
    }
    #endregion

    public void IchorRefill(Vector3 pos, float radius, float amount)
    {
        foreach (var gridPosition in farmTileMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = farmTileMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= radius && farmTileMap.GetTile(gridPosition) != null)
            {
                for(int i = 0; i < allFarmTiles.Count; i++)
                {
                    if(allFarmTiles[i] == gridPosition)
                    {
                        if(storage[i] == null) storage[i] = new NutrientStorage();

                        storage[i].ichorLevel += amount;
                        if(storage[i].ichorLevel > 10) storage[i].ichorLevel = 10;

                        StructureBehaviorScript structure = GrabStructureOnTile(tilePosition);
                        if(structure)
                        {
                            FarmLand farmPlot = structure as FarmLand;
                            if(farmPlot) farmPlot.IchorRefill();
                        }
                    } 
                }
            }
            
        }
    }

    public void NutrientRefill(Vector3 pos, float radius, float i, float t, float g)
    {
        foreach (var gridPosition in farmTileMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePosition = farmTileMap.GetCellCenterWorld(gridPosition);
            if(Vector3.Distance(tilePosition, pos) <= radius && farmTileMap.GetTile(gridPosition) != null)
            {
                for(int x = 0; x < allFarmTiles.Count; x++)
                {
                    if(allFarmTiles[x] == gridPosition)
                    {
                        if(storage[x] == null) storage[x] = new NutrientStorage();

                        storage[x].ichorLevel += i;
                        if(storage[x].ichorLevel > 10) storage[x].ichorLevel = 10;

                        storage[x].terraLevel += t;
                        if(storage[x].terraLevel > 10) storage[x].terraLevel = 10;

                        storage[x].gloamLevel += g;
                        if(storage[x].gloamLevel > 10) storage[x].gloamLevel = 10;

                        StructureBehaviorScript structure = GrabStructureOnTile(tilePosition);
                        if(structure)
                        {
                            FarmLand farmPlot = structure as FarmLand;
                            if(farmPlot) farmPlot.RefreshNutrients();
                        }
                    } 
                }
            }
            
        }
    }

    public List<Vector3> WaterGunTargets(Vector3 pos, Direction dir, int range)
    {
        Tilemap currentMap = CurrentTileMap(pos);
        if(currentMap == null) return new List<Vector3>();

        List<Vector3> newTargets = new List<Vector3>();
        Vector3Int currentPos = currentMap.WorldToCell(pos);

        //for 1 tile offset
        if(dir == Direction.North)
        {
            currentPos = new Vector3Int(currentPos.x, currentPos.y + 1);
        }
        if(dir == Direction.East)
        {
            currentPos = new Vector3Int(currentPos.x + 1, currentPos.y);
        }
        if(dir == Direction.South)
        {
            currentPos = new Vector3Int(currentPos.x, currentPos.y - 1);
        }
        if(dir == Direction.West)
        {
            currentPos = new Vector3Int(currentPos.x - 1, currentPos.y);
        }

        for(int i = 0; i < range; i++)
        {
            if(dir == Direction.North)
            {
                currentPos = new Vector3Int(currentPos.x, currentPos.y + 1);
                newTargets.Add(currentMap.GetCellCenterWorld(currentPos));
            }
            if(dir == Direction.East)
            {
                currentPos = new Vector3Int(currentPos.x + 1, currentPos.y);
                newTargets.Add(currentMap.GetCellCenterWorld(currentPos));
            }
            if(dir == Direction.South)
            {
                currentPos = new Vector3Int(currentPos.x, currentPos.y - 1);
                newTargets.Add(currentMap.GetCellCenterWorld(currentPos));
            }
            if(dir == Direction.West)
            {
                currentPos = new Vector3Int(currentPos.x - 1, currentPos.y);
                newTargets.Add(currentMap.GetCellCenterWorld(currentPos));
            }
        }

        return newTargets;
    }

    public StructureBehaviorScript GrabStructureOnTile(Vector3 pos)
    {
        Collider[] hitColliders = Physics.OverlapSphere(pos, 1);
        foreach(Collider collider in hitColliders)
        {
            if(collider.gameObject.GetComponentInParent<StructureBehaviorScript>())
            {
                return collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            }
        }
        return null;
    } //used to play ichor particle

    void InstantiateNutrientStorage()
    {
        foreach (var gridPosition in farmTileMap.cellBounds.allPositionsWithin)
        {
            if(farmTileMap.GetTile(gridPosition) != null)
            {
                allFarmTiles.Add(gridPosition);
                NutrientStorage newStorage = new NutrientStorage();
                storage.Add(newStorage);
            }
        }
    }

    public NutrientStorage FetchNutrient(Vector3 pos)
    {
        Vector3Int gridPos = farmTileMap.WorldToCell(pos);
        for(int i = 0; i < allFarmTiles.Count; i++)
        {
            if(allFarmTiles[i] == gridPos)
            {
                if(storage[i] != null) return storage[i];
                else
                {
                    storage[i] = new NutrientStorage();
                    return storage[i];
                }
            } 
        }
        //if its not in the list. None of this should ever have to be called since we are instantiating the storages at scene start
        Debug.Log("We have a storage problem");
        allFarmTiles.Add(gridPos);
        NutrientStorage newStorage = new NutrientStorage();
        storage.Add(newStorage);
        return newStorage;
    }

    public void UpdateStorage(Vector3 pos, NutrientStorage s)
    {
        Vector3Int gridPos = farmTileMap.WorldToCell(pos);
        for(int i = 0; i < allFarmTiles.Count; i++)
        {
            if(allFarmTiles[i] == gridPos) storage[i].LoadStorage(storage[i], s.ichorLevel, s.terraLevel, s.gloamLevel, s.waterLevel);
        }
    }

    IEnumerator SpawnStartingStructures()
    {
        StartCoroutine(PopulateTrees(18, 27, farmTileMap));
        StartCoroutine(PopulateTrees(1, 2, barnTileMap)); //This will surely clip inside of the barn
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(PopulateStructure(15, 25, weedTile, false, farmTileMap));
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(PopulateStructure(15, 25, boulder, true, farmTileMap));
        StartCoroutine(PopulateStructure(2, 5, boulder, true, barnTileMap));
        StartCoroutine(PopulateStructure(1, 2, barricade, true, barnTileMap));
        StartCoroutine(Populate1X2Structure(1, 1, trough, barnTileMap));
        StartCoroutine(PopulateStructure(1, 2, critterHive, true, barnTileMap));
        StartCoroutine(PopulateStructure(1, 1, wBearTrap, true, farmTileMap));
        StartCoroutine(PopulateStructure(1, 1, bearTrap, true, farmTileMap));
        PopulateBerryBushes(2, 3, true);
    }

    IEnumerator PopulateStructure(int min, int max, GameObject prefab, bool randomizeRotation, Tilemap tileMap)
    {
        List<Vector3Int> spawnablePositions = new List<Vector3Int>();

        Vector3 spawnPos = new Vector3 (0,0,0);
        foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
        {
            if(tileMap.GetTile(position) == freeTile) spawnablePositions.Add(position);
        }

        int r = Random.Range(min,max + 1);
        if (r <= 0) yield break;
        for(int i = 0; i < r; i++)
        {
            if(spawnablePositions.Count != 0)
            {
                int randomIndex = Random.Range(0, spawnablePositions.Count);
                spawnPos = tileMap.GetCellCenterWorld(spawnablePositions[randomIndex]);

                if(tileMap.GetTile(spawnablePositions[randomIndex]) != null && tileMap.GetTile(spawnablePositions[randomIndex]) != occupiedTile)
                {
                    GameObject newStruct = SpawnStructureWithInstance(prefab, spawnPos);
                    if(randomizeRotation)
                    {
                        int n = Random.Range(0,4);

                        switch(n)
                        {
                            case 0:
                            break;
                            case 1:
                            newStruct.transform.Rotate(0, 90, 0);
                            break;
                            case 2:
                            newStruct.transform.Rotate(0, 180, 0);
                            break;
                            case 3:
                            newStruct.transform.Rotate(0, 270, 0);
                            break;
                        }
                    }

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    IEnumerator Populate1X2Structure(int min, int max, GameObject prefab, Tilemap tileMap)
    {
        List<Vector3Int> spawnablePositions = new List<Vector3Int>();

        Vector3 spawnPos = new Vector3 (0,0,0);
        foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
        {
            if(tileMap.GetTile(position) == freeTile) spawnablePositions.Add(position);
        }

        int r = Random.Range(min,max + 1);
        if (r <= 0) yield break;
        float i = 0;
        while(i < r)
        {
            if(spawnablePositions.Count != 0)
            {
                int randomIndex = Random.Range(0, spawnablePositions.Count);
                spawnPos = tileMap.GetCellCenterWorld(spawnablePositions[randomIndex]);

                if(tileMap.GetTile(spawnablePositions[randomIndex]) != null)
                {
                    bool success = Spawn1X2Structure(prefab, spawnPos);
                    i++;
                    yield return new WaitForSeconds(0.01f);
                    //print(success);
                }
                else i += 0.25f;
            }
            else i += 0.25f;
        }

    }

    IEnumerator PopulateTrees(int min, int max, Tilemap tileMap)
    {
        List<Vector3Int> spawnablePositions = new List<Vector3Int>();

        Vector3 spawnPos = new Vector3 (0,0,0);
        foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
        {
            spawnablePositions.Add(position);
        }

        int r = Random.Range(min,max + 1);
        if (r <= 0) yield break;
        float i = 0;
        while(i < r)
        {
            if(spawnablePositions.Count != 0)
            {
                int randomIndex = Random.Range(0, spawnablePositions.Count);
                spawnPos = tileMap.GetCellCenterWorld(spawnablePositions[randomIndex]);

                if(tileMap.GetTile(spawnablePositions[randomIndex]) != null)
                {
                    bool success = SpawnLargeStructure(farmTree, spawnPos, true);
                    i++;
                    yield return new WaitForSeconds(0.01f);
                    //print(success);
                }
                else i += 0.25f;
            }
            else i += 0.25f;
        }
    }

    void PopulateForageables(int min, int max)
    {
        int r = Random.Range(min,max + 1);
        int p = 0;
        float x, z;

        StructurePoolManager pool = StructurePoolManager.Instance;
        bool canSpawn;

        if (r <= 0) return;
        for(int i = 0; i < r; i++)
        {
            canSpawn = true;
            int t = 0;
            p = Random.Range(0, pool.forageableSpots.Length);
            Vector3 spawnPos = pool.forageableSpots[p].position;
            x = Random.Range(-5, 5);
            z = Random.Range(-5, 5);
            spawnPos = new Vector3(spawnPos.x + x, spawnPos.y, spawnPos.z + z);

            Collider[] hitPlants = Physics.OverlapSphere(transform.position, 3.5f, 1 << 6);
            foreach(Collider collider in hitPlants)
            {
                Forgeable structure = collider.gameObject.GetComponentInParent<Forgeable>();
                if(structure)
                {
                    canSpawn = false;
                    break;
                }
            }
            if(canSpawn)
            {
                GameObject newStructure = pool.GrabForageable(false);
                newStructure.transform.position = spawnPos;
            }

        }
    }

    void PopulateNightWeeds(int min, int max)
    {
        List<Vector3Int> spawnablePositions = new List<Vector3Int>();

        Vector3 spawnPos = new Vector3 (0,0,0);
        foreach (Vector3Int position in farmTileMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePos = farmTileMap.GetCellCenterWorld(position);
            if(farmTileMap.GetTile(position) == freeTile && FetchNutrient(tilePos).ichorLevel >= 4)
            {
                spawnablePositions.Add(position);
            }
        }

        int r = Random.Range(min,max + 1);
        if (r <= 0) return;
        for(int i = 0; i < r; i++)
        {
            if(spawnablePositions.Count != 0)
            {
                int randomIndex = Random.Range(0, spawnablePositions.Count);
                spawnPos = farmTileMap.GetCellCenterWorld(spawnablePositions[randomIndex]);

                if(farmTileMap.GetTile(spawnablePositions[randomIndex]) != null && farmTileMap.GetTile(spawnablePositions[randomIndex]) != occupiedTile)
                {
                    FarmLand script = Instantiate(farmTile, spawnPos, Quaternion.identity).GetComponent<FarmLand>();
                    script.InsertCrop(fogChime);
                    SetTile(spawnPos);
                }
                spawnablePositions.RemoveAt(randomIndex);
            }
        }
    }

    void PopulateBerryBushes(int min, int max, bool harvestable)
    {
        List<Vector3Int> spawnablePositions = new List<Vector3Int>();

        Vector3 spawnPos = new Vector3 (0,0,0);
        foreach (Vector3Int position in farmTileMap.cellBounds.allPositionsWithin)
        {
            Vector3 tilePos = farmTileMap.GetCellCenterWorld(position);
            if(farmTileMap.GetTile(position) == freeTile && FetchNutrient(tilePos).gloamLevel >= 6)
            {
                spawnablePositions.Add(position);
            }
        }

        int r = Random.Range(min,max + 1);
        if (r <= 0) return;
        for(int i = 0; i < r; i++)
        {
            if(spawnablePositions.Count != 0)
            {
                int randomIndex = Random.Range(0, spawnablePositions.Count);
                spawnPos = farmTileMap.GetCellCenterWorld(spawnablePositions[randomIndex]);

                if(farmTileMap.GetTile(spawnablePositions[randomIndex]) != null && farmTileMap.GetTile(spawnablePositions[randomIndex]) != occupiedTile)
                {
                    FarmLand script = Instantiate(farmTile, spawnPos, Quaternion.identity).GetComponent<FarmLand>();
                    script.InsertCrop(berryBush);
                    SetTile(spawnPos);
                    if(harvestable)
                    {
                        if(Random.Range(0,10) <= 2) script.growthStage = 4;
                        else
                        {
                            script.growthStage = berryBush.harvestableGrowthStages[0];
                            script.harvestable = true;
                        }
                        script.SpriteChange();
                    }
                }
                spawnablePositions.RemoveAt(randomIndex);
            }
        }
    }

    void PopulateDecorCrows(int min, int max)
    {
        int r = Random.Range(min,max + 1);
        //print(r);
        if (r <= 0) return;
        Transform lastTransform = null;
        for(int i = 0; i < r; i++)
        {
            int s = Random.Range(0, StructurePoolManager.Instance.crowSpots.Length);
            Transform chosenPoint = StructurePoolManager.Instance.crowSpots[s];
            if(lastTransform == null || chosenPoint != lastTransform)
            {
                lastTransform = chosenPoint;
                Instantiate(crowPod, chosenPoint.transform.position, Quaternion.identity);
                print("Spawned");
            }
        }
    }

    public void WeedSpread(Vector3 pos, out bool becomeThorn)
    {
        becomeThorn = false;
        int weedTotal = 0;
        for(int i = 0; i < allStructs.Count; i++)
        {
            FarmLand weedScript = allStructs[i] as FarmLand;
            if(weedScript && weedScript.isWeed) weedTotal++;
        }
        if(weedTotal > 80) return;

        List<Vector3> weedSpots = GetAdjacentClearTiles(pos);
        if(weedSpots.Count == 0)
        {
            if(Random.Range(0f, 10f) > 9.5f) becomeThorn = true;
            return;
        } 
        foreach(Vector3 weedPos in weedSpots)
        {
            if(Random.Range(0f,10f) > 9.7f)
            {
                SpawnStructure(weedTile, weedPos);
                break;
            }
        }
    }

    public void IncreaseNutrients() //Occurs every new day
    {
        for(int i = 0; i < storage.Count; i++)
        {
            if(storage[i] != null)
            {
                storage[i].gloamLevel += 0.25f;
                if(storage[i].gloamLevel > 10) storage[i].gloamLevel = 10;
                storage[i].terraLevel += 0.25f;
                if(storage[i].terraLevel > 10) storage[i].terraLevel = 10;
            }
        }
        //
        for(int i = 0; i < allStructs.Count; i++)
        {
            FarmLand farmTile = allStructs[i] as FarmLand;
            if(farmTile) farmTile.RefreshNutrients();
        }
    }

    public Vector3 FindFreeTileNearCrop()
    {
        List<Vector3> cropTiles = new List<Vector3>();

        for(int i = 0; i < allStructs.Count; i++)
        {
            FarmLand farmTile = allStructs[i] as FarmLand;
            if(farmTile && !farmTile.isWeed && farmTile.crop && !farmTile.rotted) cropTiles.Add(GetTileCenter(farmTile.transform.position));
        }
        if(cropTiles.Count > 0)
        {
            int x = 0;
            List<Vector3> clearTiles = new List<Vector3>();
            while(x < 50)
            {
                int r = Random.Range(0, cropTiles.Count);
                clearTiles = GetAdjacentClearTiles(cropTiles[r]);
                if(clearTiles.Count > 0)
                {
                    return clearTiles[Random.Range(0,clearTiles.Count)];
                }

                x++;
            }
            //code for replacing a crop
        }

        return GetRandomClearTile();
    }

    public Transform FindBurrow(bool returnFarthest, Vector3 pos)
    {
        List<Transform> burrows = new List<Transform>();

        for(int i = 0; i < allStructs.Count; i++)
        {
            Burrow burrow = allStructs[i] as Burrow;
            if(burrow) burrows.Add(burrow.transform);
        }

        if(burrows.Count > 0)
        {
            if(returnFarthest)
            {
                Transform furthestBurrow = null;
                float minDistance = 25;
                for(int i = 0; i < burrows.Count; i++)
                {
                    float dist = Vector3.Distance(pos, burrows[i].transform.position);
                    if(dist > minDistance)
                    {
                        furthestBurrow = burrows[i];
                        minDistance = dist;
                    }
                }
                return furthestBurrow;
            }
            else
            {
                return burrows[Random.Range(0, burrows.Count)];
            }
        }

        return null;
    }

    public int BurrowCount()
    {
        List<Transform> burrows = new List<Transform>();

        for(int i = 0; i < allStructs.Count; i++)
        {
            Burrow burrow = allStructs[i] as Burrow;
            if(burrow) burrows.Add(burrow.transform);
        }
        return burrows.Count;
    }

    public int TallyStructure(StructureObject data)
    {
        int x = 0;
        for(int i = 0; i < allStructs.Count; i++)
        {
            if(allStructs[i].structData && allStructs[i].structData == data) x++;
        }
        return x;
    }

    public List<GameObject> ReturnStructuresOfType(StructureObject data)
    {
        List<GameObject> temp = new List<GameObject>();
        for(int i = 0; i < allStructs.Count; i++)
        {
            if(allStructs[i].structData && allStructs[i].structData == data) temp.Add(allStructs[i].gameObject);
        }
        return temp;
    }
}

[System.Serializable]
public class NutrientStorage
{
    public float ichorLevel = 0; //max is 10
    public float terraLevel = 10; //max is 10
    public float gloamLevel = 10; //max is 10

    public float waterLevel = 3; //max is 10

    public int blightLevel = 0; //max is 10. If above 0, spreads blight to nearby tiles. Corpses that die should blight farm tiles and create empty farm tiles with blight. Blighted farm tiles shouldnt despawn

    public NutrientStorage()
    {
        ichorLevel = 0; 
        terraLevel = 10; 
        gloamLevel = 10; 
        waterLevel = 3;
    }

    public void ResetStorage(NutrientStorage s)
    {
        s.ichorLevel = 0;
        s.terraLevel = 10;
        s.gloamLevel = 10;
        s.waterLevel = 3;
    }
    public void LoadStorage(NutrientStorage s, float i, float t, float g, float w)
    {
        s.ichorLevel = i;
        s.terraLevel = t;
        s.gloamLevel = g;
        s.waterLevel = w;
    }
}

public enum Direction
{
    North,
    East,
    South,
    West,
    Null
}
[System.Serializable]
public enum GridType
{
    Any,
    Farm,
    Cabin,
    Town, //Will most likely go unused and count as farm
    Barn,
    Decor
}
