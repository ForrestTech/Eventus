using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace EventSourcing.Tests.Unit.Fixture
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