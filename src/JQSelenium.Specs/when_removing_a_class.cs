using Machine.Specifications;

namespace JQSelenium.Specs
{
    public class when_removing_a_class : given_a_jquery_factory_context
    {
        static JQuerySelector _jqs;
        static string _className;
        static string _newClassName;

        Establish context = () =>
            {
                _newClassName = "randomClass";
                _className = "jq-clearfix";
                _jqs = JQuery.Find("." + _className);
            };

        Because of = () => _jqs.RemoveClass();

        It should_also_remove_the_other_class = () => _jqs.HasClass(_newClassName).ShouldBeFalse();

        It should_remove_a_class = () => _jqs.HasClass(_className).ShouldBeFalse();
    }
}