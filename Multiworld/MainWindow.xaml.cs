using Bartz24.Data;
using Multiworld.MemoryScanner.memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Multiworld;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Process LRFF13;
    private readonly Scanner LRFF13Scanner;
    private long treasuresStartPtr, multiTreasureNamePtr, multiTreasureItemPtr;

    public static List<string> ItemsList { get; set; } = new List<string>();
    public static List<string> QueueList { get; set; } = new List<string>();
    public MainWindow()
    {
        DataExtensions.Mode = ByteMode.LittleEndian;
        Process[] processes = Process.GetProcessesByName("LRFF13");
        if (processes.Length > 0)
        {
            LRFF13 = processes[0];
            LRFF13Scanner = new Scanner(LRFF13, LRFF13.Handle);
            LRFF13Scanner.setModule(LRFF13.MainModule);
        }
        else
        {
            Close();
        }

        InitializeComponent();
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new();
        dispatcherTimer.Tick += dispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        dispatcherTimer.Start();

        DataContext = this;

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        QueueList.Add(addItemTextBox.Text);
    }

    private void dispatcherTimer_Tick(object sender, EventArgs e)
    {

        if (treasuresStartPtr == 0)
        {
            long ptr1 = Memory.ReadUInt(LRFF13.Handle, (long)LRFF13.MainModule.BaseAddress + 0x4CF896C);
            long ptr2 = Memory.ReadUInt(LRFF13.Handle, ptr1 + 0x4);
            long ptr3 = Memory.ReadUInt(LRFF13.Handle, ptr2 + 0x310);
            treasuresStartPtr = Memory.ReadUInt(LRFF13.Handle, ptr3 + 0x1C);
            for (int i = 0; i < 50000; i++)
            {
                string firstChar = Memory.ReadString(LRFF13.Handle, treasuresStartPtr + i, 1);
                string str = Memory.ReadString(LRFF13.Handle, treasuresStartPtr + i, 16);
                if (str == "ran_multi" && firstChar == "r")
                {
                    multiTreasureNamePtr = treasuresStartPtr + i;
                }

                if (str == "rando_multi_item" && firstChar == "r")
                {
                    multiTreasureItemPtr = treasuresStartPtr + i;
                }
            }
        }

        long pSomeStatsBase = Memory.ReadUInt(LRFF13.Handle, (long)LRFF13.MainModule.BaseAddress + 0x4CF79D8);
        long recoveryItemOffset = 0x1418;
        long keyItemsPointer = Memory.ReadUInt(LRFF13.Handle, pSomeStatsBase + recoveryItemOffset + 0x174);
        List<string> items = new();

        for (int i = 0; i < 100; i++)
        {
            string item = Memory.ReadString(LRFF13.Handle, keyItemsPointer + (24 * i), 16);
            byte count = Memory.ReadByte(LRFF13.Handle, keyItemsPointer + (24 * i) + 18);
            if (count > 0)
            {
                items.Add(item + ": " + count);
            }

            if (item == "key_r_multi" && count > 0 && QueueList.Count > 0)
            {
                LRFF13Scanner.writeBytes(LRFF13, (ulong)(keyItemsPointer + (24 * i) + 18), new byte[] { 0, 0, 0, 0 });

                UpdateTreasure("rando_multi_item", 9999);
            }
        }

        ItemsList = items;

        long multiTreasurePtr = Memory.ReadUInt(LRFF13.Handle, multiTreasureNamePtr + 16);
        string tItem = Memory.ReadString(LRFF13.Handle, multiTreasureItemPtr, 16);
        Memory.ReadUInt(LRFF13.Handle, multiTreasurePtr);
        if (tItem == "rando_multi_item")
        {
            if (QueueList.Count > 0)
            {
                string item = QueueList[0].Substring(0, QueueList[0].IndexOf(":"));
                uint count = uint.Parse(QueueList[0].Substring(QueueList[0].IndexOf(":") + 1));
                UpdateTreasure(item, count);
                QueueList.RemoveAt(0);
            }
        }

        QueueList = new List<string>(QueueList);
        queueListBox.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();
    }

    private void UpdateTreasure(string item, uint count)
    {
        long multiTreasurePtr = Memory.ReadUInt(LRFF13.Handle, multiTreasureNamePtr + 16);
        byte[] bytes = System.Text.Encoding.Default.GetBytes(item).Concat(Enumerable.Range(0, 16 - item.Length).Select(_ => (byte)0).ToArray());
        byte[] bytesCount = new byte[4];
        bytesCount.SetUInt(0, count);

        LRFF13Scanner.writeBytes(LRFF13, (ulong)multiTreasureItemPtr, bytes);
        LRFF13Scanner.writeBytes(LRFF13, (ulong)multiTreasurePtr, bytesCount);
    }

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {

        if (!e.Handled)
        {
            e.Handled = true;
            MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            UIElement parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}
