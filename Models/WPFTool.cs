using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ZSubtitle
{
	class WPFTool
	{
		public static bool IsInstanceOrSubclass<T>(Type type)
		{
			return typeof(T) == type || type.IsSubclassOf(typeof(T));
		}
		public static T FindElement<T>(UIElement root) where T : UIElement
		{
			if (root == null) return null;

			T output = null;
			Type rootType = root.GetType();

			if (rootType == typeof(T)) return (T)root;

			if (typeof(IAddChild).IsAssignableFrom(rootType))
			{
				if (IsInstanceOrSubclass<Panel>(rootType))
				{
					foreach (UIElement ui in (root as Panel).Children)
					{
						output = FindElement<T>(ui);
						if (output != null) return output;
					}
				}
				else if (IsInstanceOrSubclass<ItemsControl>(rootType))
				{
					var generator = (root as ItemsControl).ItemContainerGenerator;
					for (int i = 0; i < generator.Items.Count; i++)
					{
						var template = generator.ContainerFromIndex(i) as FrameworkElement;
						output = FindElement<T>(template);
						if (output != null) return output;
					}
				}
				else if (IsInstanceOrSubclass<ContentControl>(rootType))
				{
					var template = (root as ContentControl).Content as FrameworkElement;
					return FindElement<T>(template);
				}
			}

			return null;
		}
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}
		public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
		{
			foreach (T child in FindVisualChildren<T>(obj))
			{
				return child;
			}

			return null;
		}
	}
}
