using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Fibertest.Dto;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Here we react on changes in Fibers collection in GraphReadModel
    /// applying those to the markers(routes) collection of map
    /// </summary>
    public partial class MapUserControl
    {
        private void FibersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ApplyAddedFibers(e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ApplyRemovedFibers(e.OldItems!);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyAddedFibers(IList newItems)
        {
            foreach (var newItem in newItems)
            {
                var fiberVm = (FiberVm)newItem;
                fiberVm.PropertyChanged += FiberVm_PropertyChanged;

                CreateRoute(fiberVm);
            }
        }

        private void CreateRoute(FiberVm fiberVm)
        {
            var points = new List<PointLatLng>() { fiberVm.Node1.Position, fiberVm.Node2.Position };

            var route = new GMapRoute(fiberVm.Id, fiberVm.Node1.Id, fiberVm.Node2.Id,
                 fiberVm.State.GetBrush(isForeground: true), fiberVm.IsBadSegment ? 4 : 2, points, MainMap);

            if (fiberVm.Node1.Id.ToString().StartsWith(@"69a2499f") || fiberVm.Node2.Id.ToString().StartsWith(@"69a2499f"))
            {
                Console.WriteLine(@"Fi-fi-fam!");
            }
            
            route.PropertyChanged += Route_PropertyChanged;
            route.Shape.Visibility = GraphReadModel.SelectedGraphVisibilityItem.Level >= GraphVisibilityLevel.RtuAndTraces
                ? Visibility.Visible
                : Visibility.Hidden;
            MainMap.Markers.Add(route);
        }


        private void FiberVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var fiberVm = (FiberVm)sender!;

            var oldRoute = (GMapRoute?)MainMap.Markers.FirstOrDefault(r => r.Id == fiberVm.Id);
            if (oldRoute != null)
                MainMap.Markers.Remove(oldRoute);

            CreateRoute(fiberVm);
        }

        private void ApplyRemovedFibers(IList oldItems)
        {
            foreach (var oldItem in oldItems)
            {
                var fiberVm = (FiberVm)oldItem;
                var route = MainMap.Markers.First(r => r.Id == fiberVm.Id);
                MainMap.Markers.Remove(route);
            }
        }

    }
}
