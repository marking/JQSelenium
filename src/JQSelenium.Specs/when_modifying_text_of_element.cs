﻿using Machine.Specifications;

namespace JQSelenium.Specs
{
    internal class when_modifying_text_of_element : given_a_jquery_factory_context
    {
        const string TestString = "testString";
        static JQuerySelector _result;

        Because of = () => _result = JQuery.Find("body").Get().Text(TestString);

        It should_return_the_modified_element = () => _result.Text().ShouldEqual(TestString);
    }
}