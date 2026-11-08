namespace YouSummarize.Data.EntityModel.Blocks;

public class BlockInstructionEntity : BlockBaseEntity
{
    public string Instruction { get; set; } = string.Empty;

    public BlockInstructionEntity()
    {
        HalfPage = false;
    }
}
