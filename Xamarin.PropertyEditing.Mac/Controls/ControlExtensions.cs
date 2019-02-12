using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal static  class ControlExtensions
	{
		public static Task<ITypeInfo> RequestAt (this TypeRequestedEventArgs self, IHostResourceProvider hostResources, NSView source, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			var tcs = new TaskCompletionSource<ITypeInfo> ();

			var vm = new TypeSelectorViewModel (assignableTypes);
			var selector = new TypeSelectorControl {
				ViewModel = vm,
				Appearance = source.EffectiveAppearance
			};

			vm.PropertyChanged += (vms, ve) => {
				if (ve.PropertyName == nameof (TypeSelectorViewModel.SelectedType)) {
					tcs.TrySetResult (vm.SelectedType);
				}
			};

			var popover = new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				Delegate = new PopoverDelegate<ITypeInfo> (tcs),
				ContentViewController = new NSViewController {
					View = selector,
					PreferredContentSize = new CoreGraphics.CGSize (360, 335)
				},
			};
			popover.SetAppearance (hostResources.GetVibrantAppearance (source.EffectiveAppearance));

			tcs.Task.ContinueWith (t => {
				popover.PerformClose (popover);
				popover.Dispose ();
			}, TaskScheduler.FromCurrentSynchronizationContext ());

			popover.Show (source.Frame, source.Superview, NSRectEdge.MinYEdge);
			return tcs.Task;
		}

		private class PopoverDelegate<T>
			: NSPopoverDelegate
		{
			public PopoverDelegate (TaskCompletionSource<T> tcs)
			{
				this.tcs = tcs;
			}

			public override void WillClose (NSNotification notification)
			{
				this.tcs.TrySetCanceled ();
			}

			private readonly TaskCompletionSource<T> tcs;
		}
	}
}
