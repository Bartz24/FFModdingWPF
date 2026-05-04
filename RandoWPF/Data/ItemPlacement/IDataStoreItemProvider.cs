namespace Bartz24.RandoWPF;
public interface IDataStoreItemProvider<T>
{
    public T GetItemData(bool orig);
}
