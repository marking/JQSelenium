using Machine.Specifications;

namespace JQSelenium.Specs
{
    internal class when_getting_the_children_of_a_selector : given_a_jquery_factory_context
    {
        static JQuerySelector _contextSelector;
        static JQuerySelector _expectedChildren;
        static JQuerySelector _result;

        Establish context = () =>
            {
                _contextSelector = jQueryFactory.Query("div#jq-primaryNavigation");
                _expectedChildren = new JQuerySelector(jQueryFactory.Query("ul").first());
            };

        Because of = () => { _result = _contextSelector.children(); };

        It should_return_all_its_children = () => _result.hasSameElementsOf(_expectedChildren).ShouldBeTrue();
    }
}