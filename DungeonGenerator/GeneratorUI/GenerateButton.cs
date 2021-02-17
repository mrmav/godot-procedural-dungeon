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

        float edgeShare = (float)GetNode<SpinBox>("../Form/MinEdgeSharing/SpinBox").Value;

        int algorithm = GetNode<OptionButton>("../Form/PathFindingAlgo/OptionButton").Selected;
        
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
            (DungeonHeuristic)algorithm,
            seed
        );

        EmitSignal(nameof(GenerateNewDungeon), p);
        
    }

}
