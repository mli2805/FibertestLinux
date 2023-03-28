using System;
using System.Windows.Media.Imaging;
using Fibertest.Dto;

namespace Fibertest.WpfClient;

public static class EquipmentTypeWpfExt
{
    public static BitmapImage GetNodePictogram(EquipmentType type, FiberState state)
    {
        return type == EquipmentType.AccidentPlace ?
            GetAccidentPictogramImage(state) :
            GetOkNodePictogramBitmapImage(type);
    }

    private static BitmapImage GetOkNodePictogramBitmapImage(EquipmentType type)
    {
        string stateName = FiberState.Ok.ToString();
        string typeName = type.ToString();
        var path = $@"pack://application:,,,/Resources/{typeName}/{typeName}{stateName}.png";
        return new BitmapImage(new Uri(path));
    }

    private static BitmapImage GetAccidentPictogramImage(FiberState state)
    {
        string stateName;
        switch (state)
        {
            case FiberState.Minor: stateName = @"Minor"; break;
            case FiberState.Major: stateName = @"Major"; break;
            default: stateName = @"Critical"; break;
        }
        return new BitmapImage(new Uri($@"pack://application:,,,/Resources/OnMap/{stateName}AccidentPlace.png"));
    }

}