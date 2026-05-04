namespace Bartz24.RandoWPF;
public class ItemReqComponent : ItemLocationReqComponent
{
    private ItemReq Requirements { get; set; }

    public ItemReqComponent(ItemReq req)
    {
        Requirements = req;
    }

    public override bool AreItemReqsMet(ProgressionState state)
    {
        return Requirements.IsValid(state);
    }
}
