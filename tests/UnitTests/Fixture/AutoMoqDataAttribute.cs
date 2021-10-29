namespace Eventus.UnitTests.Fixture
{
    using AutoFixture.AutoMoq;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using Moq;
    using Storage;

    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        private static AutoFixture.Fixture CustomFixture
        {
            get
            {
                var fixture = new AutoFixture.Fixture();
                fixture.Customize(new AutoMoqCustomization());
                fixture.Customize<Repository>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                return fixture;
            }
        }
        public AutoMoqDataAttribute()
            : base(() => CustomFixture)
        {
        }
    }
}