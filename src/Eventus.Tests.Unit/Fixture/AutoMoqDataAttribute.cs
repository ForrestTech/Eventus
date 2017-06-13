using Eventus.Storage;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit2;

namespace Eventus.Tests.Unit.Fixture
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        private static Ploeh.AutoFixture.Fixture CustomFixture
        {
            get
            {
                var fixture = new Ploeh.AutoFixture.Fixture();
                fixture.Customize(new AutoMoqCustomization());
                fixture.Customize<Repository>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                return fixture;
            }
        }
        public AutoMoqDataAttribute()
            : base(CustomFixture)
        {
        }
    }
}