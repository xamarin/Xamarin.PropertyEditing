using System;
using System.Linq;
using System.Linq.Expressions;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	public struct ConstraintProxy
	{
		public int Left, Top, Right, Bottom, Leading, Trailing, CenterX, CenterY, Baseline, Height, Width;
	}

	public static class ConstraintExtensions
	{
		public static NSLayoutConstraint[] ConstraintFill (this NSView child, NSView container, int padding = 0)
		{
			return new NSLayoutConstraint[] {
				NSLayoutConstraint.Create (child, NSLayoutAttribute.Left, NSLayoutRelation.Equal, container, NSLayoutAttribute.Left, 1, padding),
				NSLayoutConstraint.Create (child, NSLayoutAttribute.Right, NSLayoutRelation.Equal, container, NSLayoutAttribute.Right, 1, -padding),
				NSLayoutConstraint.Create (child, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1, container.IsFlipped ? padding : -padding),
				NSLayoutConstraint.Create (child, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, container, NSLayoutAttribute.Bottom, 1, container.IsFlipped ? -padding : padding),
			};
		}

		public static NSLayoutConstraint ConstraintTo (this NSView view1, NSView view2, Expression<Func<ConstraintProxy, ConstraintProxy, bool>> expression)
		{
			using (Performance.StartNew (view1.GetType().Name)) {
				var mainExpression = expression.Body as BinaryExpression;
				NSLayoutRelation relation = NSLayoutRelation.Equal;
				switch (mainExpression.NodeType) {
				case ExpressionType.Equal:
					relation = NSLayoutRelation.Equal;
					break;
				case ExpressionType.LessThanOrEqual:
					relation = NSLayoutRelation.LessThanOrEqual;
					break;
				case ExpressionType.GreaterThanOrEqual:
					relation = NSLayoutRelation.GreaterThanOrEqual;
					break;
				default:
					throw new ArgumentException ("Relation " + mainExpression.NodeType.ToString () + " not valid");
				}

				var propLeft = (NSLayoutAttribute)Enum.Parse (typeof (NSLayoutAttribute), ((MemberExpression)mainExpression.Left).Member.Name);
				var propRight = propLeft;

				if (mainExpression.Right.NodeType == ExpressionType.Constant) {
					var c = Convert.ToSingle (((ConstantExpression)mainExpression.Right).Value);
					return NSLayoutConstraint.Create (view1, propLeft, relation, null, NSLayoutAttribute.NoAttribute, 1, c);
				} else if (mainExpression.Right is MemberExpression) {
					propRight = (NSLayoutAttribute)Enum.Parse (typeof (NSLayoutAttribute), ((MemberExpression)mainExpression.Right).Member.Name);
					return NSLayoutConstraint.Create (view1, propLeft, relation, view2, propRight, 1, 0);
				}

				var addNode = mainExpression.Right as BinaryExpression;

				var mulNode = addNode;
				var constant = 0f;
				var multiplier = 1f;

				if (addNode.Left.NodeType == ExpressionType.Constant) {
					mulNode = addNode.Right as BinaryExpression;
					constant = Convert.ToSingle (((ConstantExpression)addNode.Left).Value);
				} else {
					mulNode = addNode.Left as BinaryExpression;
					constant = Convert.ToSingle (((ConstantExpression)addNode.Right).Value);
				}
				constant *= addNode.NodeType == ExpressionType.Subtract ? -1 : 1;

				if (mulNode != null) {
					if (mulNode.Left.NodeType == ExpressionType.Constant) {
						multiplier = Convert.ToSingle (((ConstantExpression)mulNode.Left).Value);
						propRight = (NSLayoutAttribute)Enum.Parse (typeof (NSLayoutAttribute), ((MemberExpression)mulNode.Right).Member.Name);
					} else {
						multiplier = Convert.ToSingle (((ConstantExpression)mulNode.Right).Value);
						propRight = (NSLayoutAttribute)Enum.Parse (typeof (NSLayoutAttribute), ((MemberExpression)mulNode.Left).Member.Name);
					}
					if (mulNode.NodeType == ExpressionType.Divide)
						multiplier = 1.0f / multiplier;
				} else {
					var member = (MemberExpression)(addNode.Right is MemberExpression ? addNode.Right : addNode.Left);
					propRight = (NSLayoutAttribute)Enum.Parse (typeof (NSLayoutAttribute), member.Member.Name);
				}

				//Console.WriteLine ("v1.{0} {1} v2.{2} * {3} + {4}", propLeft, relation, propRight, multiplier, constant);

				using (Performance.StartNew ("Create")) {
					return NSLayoutConstraint.Create (view1, propLeft, relation, view2, propRight, multiplier, constant);
				}
			}
		}

		public static void DoConstraints (this NSView view, params NSLayoutConstraint[] constraints)
		{
			using (Performance.StartNew ()) {
				NSLayoutConstraint.ActivateConstraints (constraints);
			}
			//view.AddConstraints (constraints);
		}

		public static void DoMergedConstraints (this NSView view, params object[] constraints)
		{
			foreach (var o in constraints) {
				var singleConstraint = o as NSLayoutConstraint;
				var multipleConstraint = o as NSLayoutConstraint[];
				if (singleConstraint != null)
					view.AddConstraint (singleConstraint);
				else if (multipleConstraint != null)
					NSLayoutConstraint.ActivateConstraints (multipleConstraint);
					//view.AddConstraints (multipleConstraint);
				else
					throw new ArgumentException ("Unexpected constraint type: " + o.GetType ());
			}
		}

		public static NSLayoutConstraint SticksTo (this NSView child, NSView container, NSLayoutAttribute attribute, int padding = 0)
		{
			return NSLayoutConstraint.Create (child, attribute, NSLayoutRelation.Equal, container, attribute, 1, padding);
		}

		public static NSLayoutConstraint[] SticksTo (this NSView child, NSView container, params NSLayoutAttribute[] attributes)
		{
			return attributes
					.Select (a => NSLayoutConstraint.Create (child, a, NSLayoutRelation.Equal, container, a, 1, 0))
					.ToArray ();
		}

		public static NSLayoutConstraint WithPriority (this NSLayoutConstraint constraint, NSLayoutPriority priority)
		{
			return constraint.WithPriority ((float)priority);
		}

		public static NSLayoutConstraint WithPriority (this NSLayoutConstraint constraint, float priority)
		{
			constraint.Priority = priority;
			return constraint;
		}
	}
}
