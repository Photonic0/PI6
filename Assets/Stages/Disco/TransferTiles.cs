using UnityEngine;
using UnityEngine.Tilemaps;

public class TransferTiles : MonoBehaviour
{
    //references to the information we need for the code
    [SerializeField] Tilemap mainGrid;
    [SerializeField] Tilemap backLayer;
    // the [] indicates that this variable is a list of TileBase variables
    //TileBase is used by other tile types so you can use it when you want
    //to store any sort of tile
    [SerializeField] TileBase[] tileTypesToReplace;

    //this I will use to create a fake button in the editor
    [SerializeField] bool debug_ClickToReplaceTiles;
    [SerializeField] bool debug_ClickToAddCaps;
    [SerializeField] TileBase leftCap, rightCap, topCap, bottomCap;
    [SerializeField] TileBase tileToAddCap;
    //this executes every frame when the game is running
    private void Update()
    {
        //this will execute everything we just wrote
        TileSwapping();
    }


    //so usualy this is for adding symbols, lines, or shapes to indicate something helpful for development
    //but we can use this to execute any code really.
    //I will be using this to execute the code without having to set the editor to play the game
    private void OnDrawGizmos()
    {
        TileSwapping();
    }

    private void TileSwapping()
    {
        //it will check for this true/false variable being true
        if (debug_ClickToReplaceTiles)
        {
            //then immediately set it to false to make it act like a button of sorts
            debug_ClickToReplaceTiles = false;

            BoundsInt bounds = mainGrid.cellBounds;

            int minI = bounds.xMin;
            int maxI = bounds.xMax;
            int minJ = bounds.yMin;
            int maxJ = bounds.yMax;

            //enter 2 loops, one that goes on the bottom most row of tiles
            //and the other goes through each column for every block in the bottom row
            //essentially iterating through the entire tilemap
            for (int i = minI; i < maxI; i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    //we will check this position in the tilemap for the tiles we want to replace
                    Vector3Int posToCheck = new(i, j);
                    TileBase tileToCheck = mainGrid.GetTile(posToCheck);
                    for (int k = 0; k < tileTypesToReplace.Length; k++)
                    {
                        TileBase tileTypeToCheck = tileTypesToReplace[k];
                        //if the tile we are checking is one of the tile types we want to replace
                        if (tileToCheck == tileTypeToCheck && tileTypeToCheck != null)
                        {
                            //set the tile of the back layer to be the tile type we are transferring
                            backLayer.SetTile(posToCheck, tileTypeToCheck);
                            //then remove the tile at the position we are checking in the main grid
                            mainGrid.SetTile(posToCheck, null);
                        }
                    }
                }
            }
        }
    }

    private void AddCaps()
    {
        if (tileToAddCap == null)
        {
            return;
        }
        if (debug_ClickToAddCaps)
        {
            debug_ClickToAddCaps = false;

            BoundsInt bounds = mainGrid.cellBounds;

            int minI = bounds.xMin + 1;
            int maxI = bounds.xMax - 1;
            int minJ = bounds.yMin + 1;
            int maxJ = bounds.yMax - 1;
            for (int i = minI; i < maxI; i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    Vector3Int posToCheck = new(i, j);
                    TileBase mainGridTile = mainGrid.GetTile(posToCheck);
                    TileBase backLayerTile = backLayer.GetTile(posToCheck);
                    if (backLayerTile == tileToAddCap)
                    {
                      

                       

                        //need to check if the tile is the + shape
                        //check tile above, to left, to right, and below for:
                        //only 1 + shaped tile
                        //if true, check if the that tile we should add the cap at, has a tile in the main grid.
                        //if not, then add the´corresponding cap
                    }
                }
            }
        }
    }
    //bool OnlyOnePlusShapedIronBeamTileAdjacent(TileBase tileToCheck, int i, int j)
    //{
    //    Vector3Int left = new(i - 1, j);
    //    Vector3Int right = new(i + 1, j);
    //    Vector3Int up = new(i, j + 1);
    //    Vector3Int down = new(i, j - 1);

    //}
}
