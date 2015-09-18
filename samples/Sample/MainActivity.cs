using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Xamarin.Android.ExpandableSelector;

namespace Sample
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light")
    ]
    public class MainActivity : Activity
    {
        private ExpandableSelector ColorsExpandableSelector
        {
            get { return (ExpandableSelector) this.FindViewById(Resource.Id.es_colors); }
        }

        private View ColorsHeaderButton
        {
            get { return this.FindViewById(Resource.Id.bt_colors); }
        }

        private ExpandableSelector IconsExpandableSelector
        {
            get { return (ExpandableSelector) this.FindViewById(Resource.Id.es_icons); }
        }

        private ExpandableSelector SizesExpandableSelector
        {
            get { return (ExpandableSelector) this.FindViewById(Resource.Id.es_sizes); }
        }

        private View CloseButton
        {
            get { return this.FindViewById(Resource.Id.bt_close); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.Main);

            this.InitializeColorsExpandableSelector();
            this.InitializeSizesExpandableSelector();
            this.InitializeIconsExpandableSelector();
            this.InitializeCloseAllButton();
        }

        private void InitializeColorsExpandableSelector()
        {
            var expandableItems = new List<ExpandableItem>
            {
                new ExpandableItem(Resource.Drawable.item_brown),
                new ExpandableItem(Resource.Drawable.item_green),
                new ExpandableItem(Resource.Drawable.item_orange),
                new ExpandableItem(Resource.Drawable.item_pink)
            };
            this.ColorsExpandableSelector.ShowExpandableItems(expandableItems);

            this.ColorsHeaderButton.Click += delegate
            {
                this.ColorsHeaderButton.Visibility = ViewStates.Visible;
                this.ColorsExpandableSelector.Expand();
            };

            this.ColorsExpandableSelector.ItemClick += (s, e) =>
            {
                switch (e.Index)
                {
                    case 0:
                        this.ColorsHeaderButton.SetBackgroundResource(Resource.Drawable.item_brown);
                        break;

                    case 1:
                        this.ColorsHeaderButton.SetBackgroundResource(Resource.Drawable.item_green);
                        break;

                    case 2:
                        this.ColorsHeaderButton.SetBackgroundResource(Resource.Drawable.item_orange);
                        break;

                    default:
                        this.ColorsHeaderButton.SetBackgroundResource(Resource.Drawable.item_pink);
                        break;
                }

                this.ColorsExpandableSelector.Collapse();
            };
        }

        private void InitializeSizesExpandableSelector()
        {
            var expandableItems = new List<ExpandableItem>
            {
                new ExpandableItem("XL"),
                new ExpandableItem("L"),
                new ExpandableItem("M"),
                new ExpandableItem("S")
            };
            this.SizesExpandableSelector.ShowExpandableItems(expandableItems);

            this.SizesExpandableSelector.ItemClick += (s, e) =>
            {
                switch (e.Index)
                {
                    case 1:
                        var firstItem = this.SizesExpandableSelector.ExpandableItems[1];
                        this.SwipeFirstItem(1, firstItem);
                        break;

                    case 2:
                        var secondItem = this.SizesExpandableSelector.ExpandableItems[2];
                        this.SwipeFirstItem(2, secondItem);
                        break;

                    case 3:
                        var fourthItem = this.SizesExpandableSelector.ExpandableItems[3];
                        this.SwipeFirstItem(3, fourthItem);
                        break;
                }


                this.SizesExpandableSelector.Collapse();
            };
        }

        private void SwipeFirstItem(int position, ExpandableItem clickedItem)
        {
            var firstItem = this.SizesExpandableSelector.ExpandableItems[0];

            this.SizesExpandableSelector.UpdateExpandableItem(0, clickedItem);
            this.SizesExpandableSelector.UpdateExpandableItem(position, firstItem);
        }

        private void InitializeIconsExpandableSelector()
        {
            var expandableItems = new List<ExpandableItem>
            {
                new ExpandableItem {ResourceId = Resource.Mipmap.ic_keyboard_arrow_up_black},
                new ExpandableItem {ResourceId = Resource.Mipmap.ic_gamepad_black},
                new ExpandableItem {ResourceId = Resource.Mipmap.ic_device_hub_black}
            };

            this.IconsExpandableSelector.ShowExpandableItems(expandableItems);

            this.IconsExpandableSelector.ItemClick += (s, e) =>
            {
                if (e.Index == 0 && this.IconsExpandableSelector.IsExpanded)
                {
                    this.IconsExpandableSelector.Collapse();
                    this.UpdateIconsFirstButtonResource(Resource.Mipmap.ic_keyboard_arrow_up_black);
                }

                switch (e.Index)
                {
                    case 1:
                        this.ShowToast("Gamepad icon button clicked.");
                        break;

                    case 2:
                        this.ShowToast("Hub icon button clicked.");
                        break;
                }
            };

            this.IconsExpandableSelector.StartExpand +=
                (s, e) => { this.UpdateIconsFirstButtonResource(Resource.Mipmap.ic_keyboard_arrow_down_black); };
        }

        private void InitializeCloseAllButton()
        {
            this.CloseButton.Click += (s, e) =>
            {
                this.ColorsExpandableSelector.Collapse();
                this.SizesExpandableSelector.Collapse();
                this.IconsExpandableSelector.Collapse();

                this.UpdateIconsFirstButtonResource(Resource.Mipmap.ic_keyboard_arrow_up_black);

                this.ShowToast("Collapse all");
            };

            this.ColorsExpandableSelector.StartCollapse +=
                (s, e) => { this.ColorsHeaderButton.Visibility = ViewStates.Visible; };
        }

        private void UpdateIconsFirstButtonResource(int resourceId)
        {
            var arrowUpExpandableItem = new ExpandableItem
            {
                ResourceId = resourceId
            };

            this.IconsExpandableSelector.UpdateExpandableItem(0, arrowUpExpandableItem);
        }

        private void ShowToast(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }
    }
}