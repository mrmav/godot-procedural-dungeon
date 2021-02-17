using Godot;

class Form : Node
{

    public override void _Ready()
    {
        // set default values:
        DungeonGenerator.DungeonParameters p = new DungeonGenerator.DungeonParameters();
        
        GetNode<SpinBox>("DungeonWidthTiles/SpinBox").Value = p.DungeonWidth;
        GetNode<SpinBox>("DungeonHeightTiles/SpinBox").Value = p.DungeonHeight;
        
        GetNode<SpinBox>("NumberOfSplits/SpinBox").Value = p.Splits;
        GetNode<SpinBox>("DeviationForSplitting/SpinBox").Value = p.SplitDeviation;
        
        GetNode<SpinBox>("MinimumRoomWidth/SpinBox").Value = p.MinRoomWidth;
        GetNode<SpinBox>("MinimumRoomHeight/SpinBox").Value = p.MinRoomHeight;

        GetNode<SpinBox>("MinEdgeSharing/SpinBox").Value = p.EdgeSharing;

        GetNode<OptionButton>("PathFindingAlgo/OptionButton").Selected = (int)p.Algorithm;
        
        GetNode<LineEdit>("Seed/LineEdit").Text = p.Seed;
       
    }

}