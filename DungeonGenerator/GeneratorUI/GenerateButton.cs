using Godot;
using DungeonGenerator;

public class GenerateButton : Button
{

    [Signal]
    public delegate void GenerateNewDungeon(DungeonParameters parameters);

    public override void _Ready()
    {
        
    }

    private void _on_Generate_pressed()
    {
        // process form values and generate a new dungeon with them

        int dungeonWidth  = (int)GetNode<SpinBox>("../Form/DungeonWidthTiles/SpinBox").Value;
        int dungeonHeight = (int)GetNode<SpinBox>("../Form/DungeonHeightTiles/SpinBox").Value;
        
        int splits = (int)GetNode<SpinBox>("../Form/NumberOfSplits/SpinBox").Value;
        float splitDeviation = (float)GetNode<SpinBox>("../Form/DeviationForSplitting/SpinBox").Value;
        
        int roomWidth  = (int)GetNode<SpinBox>("../Form/MinimumRoomWidth/SpinBox").Value;
        int roomHeight = (int)GetNode<SpinBox>("../Form/MinimumRoomHeight/SpinBox").Value;

        int edgeShare = (int)GetNode<SpinBox>("../Form/MinEdgeSharing/SpinBox").Value;
        int openness = (int)GetNode<SpinBox>("../Form/Openness/SpinBox").Value;

        int algorithm = GetNode<OptionButton>("../Form/PathFindingAlgo/OptionButton").Selected;
        int merge = GetNode<OptionButton>("../Form/MergeOption/OptionButton").Selected;
        
        string seed = GetNode<LineEdit>("../Form/Seed/LineEdit").Text;

        DungeonParameters p = new DungeonParameters
        (
            dungeonWidth,
            dungeonHeight,
            roomWidth,
            roomHeight,
            splits,
            splitDeviation,
            edgeShare,
            openness,
            (DungeonHeuristic)algorithm,
            (DungeonMergeRooms)merge,
            seed
        );

        EmitSignal(nameof(GenerateNewDungeon), p);
        
    }

}
