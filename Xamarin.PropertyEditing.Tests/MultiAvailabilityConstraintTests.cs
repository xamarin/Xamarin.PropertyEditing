using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class MultiAvailabilityConstraintTests
	{
		[TestCase (true, false, false, true, true)]
		[TestCase (true, true, false, true, true)]
		[TestCase (true, true, true, true, false)]
		[TestCase (false, false, false, false, false)]
		[TestCase (false, true, false, false, true)]
		public async Task Constraint (bool expectedResult, bool g1ar, bool g1br, bool g2ar, bool g2br)
		{
			var g1a = new Mock<IAvailabilityConstraint> ();
			g1a.Setup (a => a.GetIsAvailableAsync (null)).ReturnsAsync (g1ar);
			var g1b = new Mock<IAvailabilityConstraint> ();
			g1b.Setup (a => a.GetIsAvailableAsync (null)).ReturnsAsync (g1br);
			var g2a = new Mock<IAvailabilityConstraint> ();
			g2a.Setup (a => a.GetIsAvailableAsync (null)).ReturnsAsync (g2ar);
			var g2b = new Mock<IAvailabilityConstraint> ();
			g2b.Setup (a => a.GetIsAvailableAsync (null)).ReturnsAsync (g2br);

			var multi = new MultiAvailabilityConstraint (new[] { new[] { g1a.Object, g1b.Object }, new[] { g2a.Object, g2b.Object } });
			bool result = await multi.GetIsAvailableAsync (null);

			Assert.That (result, Is.EqualTo (expectedResult));
		}
	}
}