using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace Eventus.Tests.Unit.Fixture
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Ploeh.AutoFixture.Fixture()
                .Customize(new AutoMoqCustomization()))
        {
        }
    }
}