using Bartz24.RandoWPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Bartz24.RandoWPF.Tests;

[TestClass]
public class ItemReqTests
{
    [TestMethod]
    public void TestItemReqEquality()
    {

        ItemReq req1 = ItemReq.Item("item1", 1);
        ItemReq req2 = ItemReq.Item("item1", 1);
        ItemReq req3 = ItemReq.Item("item2", 1);
        ItemReq req4 = ItemReq.Item("item1", 2);

        ItemReq req5 = ItemReq.And(req1, req2);
        ItemReq req6 = ItemReq.And(req1, req2);
        ItemReq req7 = ItemReq.And(req1, req3);
        ItemReq req8 = ItemReq.And(req7, req4);
        ItemReq req9 = ItemReq.And(req4, req7);

        ItemReq req10 = ItemReq.Or(req1, req2);
        ItemReq req11 = ItemReq.Or(req1, req2);
        ItemReq req12 = ItemReq.Or(req1, req3);
        ItemReq req13 = ItemReq.Or(req7, req4);
        ItemReq req14 = ItemReq.Or(req4, req7);

        ItemReq req15 = ItemReq.Select(2, req1, req2, req7);
        ItemReq req16 = ItemReq.Select(2, req1, req2, req7);
        ItemReq req17 = ItemReq.Select(2, req7, req3, req1);
        ItemReq req18 = ItemReq.Select(2, req7, req1, req3);
        ItemReq req19 = ItemReq.Select(3, req1, req2, req5, req14);

        // Check ones which are equal
        Assert.IsTrue(req1 == req2);
        Assert.IsTrue(req2 == req1);
        Assert.IsTrue(req1.Equals(req2));
        Assert.IsTrue(req2.Equals(req1));
        Assert.IsTrue(req5 == req6);
        Assert.IsTrue(req6 == req5);
        Assert.IsTrue(req5.Equals(req6));
        Assert.IsTrue(req6.Equals(req5));
        Assert.IsTrue(req10 == req11);
        Assert.IsTrue(req11 == req10);
        Assert.IsTrue(req10.Equals(req11));
        Assert.IsTrue(req11.Equals(req10));
        Assert.IsTrue(req15 == req16);
        Assert.IsTrue(req16 == req15);
        Assert.IsTrue(req15.Equals(req16));
        Assert.IsTrue(req16.Equals(req15));

        List<ItemReq> falseWithEachOther = new()
        {
            req1,
            req3,
            req4,
            req5,
            req7,
            req8,
            req9,
            req10,
            req12,
            req13,
            req14,
            req15,
            req17,
            req18,
            req19
        };

        // Check ones which are not equal
        foreach (ItemReq reqLeft in falseWithEachOther)
        {
            foreach (ItemReq reqRight in falseWithEachOther)
            {
                if (reqLeft != reqRight)
                {
                    Assert.IsFalse(reqLeft.Equals(reqRight));
                    Assert.IsFalse(reqRight.Equals(reqLeft));
                }
            }
        }

    }
    
}
