using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;

namespace JQSelenium
{
    /// <summary>
    ///   It contains all the jQueryTags generated by the JQueryFactory during a Find.
    /// </summary>
    public class JQuerySelector : IEnumerable<JQueryTag>
    {
        /// <summary>
        ///   The number of elements in the JQuerySelector.
        /// </summary>
        int _count;

        /// <summary>
        ///   The selector provided by the JQueryFactory.
        /// </summary>
        string _selector;

        /// <summary>
        ///   Contains all the JQueryTags generated with the provided selector of the JQuerySelector
        /// </summary>
        List<JQueryTag> _subset;

        /// <summary>
        ///   An index used to iterate over all of the JQueryTags manually.
        /// </summary>
        int iterator;

        /// <summary>
        ///   Initializes a new JQuerySelector
        /// </summary>
        /// <summary>
        ///   Initializes a new JQuerySelector
        /// </summary>
        /// <param name="jQueryTag"> Used to create a new JQuerySelector of a single JQueryTag </param>
        public JQuerySelector(JQueryTag jQueryTag)
        {
            _subset = new List<JQueryTag> {jQueryTag};
            string[] result = jQueryTag.GetSelector().Split(']');
            if (result[result.Count() - 1].Equals(""))
            {
                _selector = "jQuery(" + jQueryTag.Selector + ")";
            }
            else
            {
                _selector = jQueryTag.Selector;
            }
            iterator = 0;
            //js = jQueryTag.GetJs();
            _count = 1;
        }

        /// <summary>
        ///   Initializes a new JQuerySelector
        /// </summary>
        /// <param name="selector"> A string containing a selector expression used for the filtered elements. </param>
        /// <param name="subset"> Contains the webElements used to create JQueryTags </param>
        public JQuerySelector(string selector, List<IWebElement> subset = null)
        {
            subset = subset ?? new List<IWebElement>();
            
            _selector = selector;
            iterator = 0;
            _subset = new List<JQueryTag>();

            for (int i = 0; i < subset.Count; i++)
            {
                try
                {
                    var jqt = new JQueryTag(selector, i, subset[i]);
                    _subset.Add(jqt);
                }
                catch (StaleElementReferenceException)
                {
                    //Do nothing.
                }
            }
            _count = _subset.Count;
        }

        #region IEnumerable<JQueryTag> Members

        public IEnumerator<JQueryTag> GetEnumerator()
        {
            for (int i = 0; i < _subset.Count; i++)
            {
                yield return _subset[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        static string Fix(string toFix)
        {
            if (RequiresApostrophe(toFix))
            {
                return "'" + toFix + "'";
            }

            return toFix;
        }

        /// <summary>
        ///   Returns the next element in the JQuerySelector.
        /// </summary>
        /// <returns> JQueryTag containing a webElement </returns>
        public JQueryTag Get()
        {
            return _subset[iterator++];
        }

        /// <summary>
        ///   Returns the element with the provided index from the JQuerySelector.
        /// </summary>
        /// <param name="index"> Position of the element in the JQuerySelector. </param>
        /// <returns> JQueryTag containing a webElement. </returns>
        public JQueryTag Get(int index)
        {
            if (index < _subset.Count)
            {
                return _subset[index];
            }
            return null;
        }

        /// <summary>
        ///   Add elements to the set of matched elements.
        ///   <para> Source: http://api.jquery.com/add/ </para>
        /// </summary>
        /// <param name="selector_elements_html">
        ///   <para> -selector: A string representing a selector expression to find additional elements to add to the set of matched elements. </para>
        ///   <para> -elements_ One or more elements to add to the set of matched elements. </para>
        ///   <para> -html: An HTML fragment to add to the set of matched elements. </para>
        /// </param>
        /// <returns> JQuerySelector </returns>
        public JQuerySelector Add(string selector_elements_html)
        {
            Object result;
            string newSelector;
            if (RequiresApostrophe(selector_elements_html))
            {
                result = ExecJs("", ".add('" + selector_elements_html + "')");
                newSelector = _selector + ".add('" + selector_elements_html + "')";
            }
            else
            {
                result = ExecJs("", ".add(" + selector_elements_html + ")");
                newSelector = _selector + ".add(" + selector_elements_html + ")";
            }
            _subset = ObjectToJQueryTagList(result);
            OverwriteSelectors(newSelector);
            _count = _subset.Count;
            return this;
        }

        /// <summary>
        ///   Add elements to the set of matched elements.
        ///   <para> Source: http://api.jquery.com/add/ </para>
        /// </summary>
        /// <param name="selector"> A string representing a selector expression to find additional elements to add to the set of matched elements. </param>
        /// <param name="context"> The point in the document at which the selector should begin matching; similar to the context argument of the $(selector, context) method. </param>
        /// <returns> JQuerySelector </returns>
        public JQuerySelector Add(string selector, string context)
        {
            var result = (Dictionary<string, Object>) ExecJs("", ".add('" + selector + "'," + context + ")");
            string newSelector = _selector + ".add('" + selector + "'," + context + ")";
            _subset = ObjectToJQueryTagList(result);
            OverwriteSelectors(newSelector);
            _count = _subset.Count;
            return this;
        }


        /// <summary>
        ///   Adds the specified class(es) to each of the set of matched elements.
        ///   <para> Source: http://api.jquery.com/addClass/ </para>
        /// </summary>
        /// <param name="className_function">
        ///   <para> -className: One or more class names to be added to the class attribute of each matched element. </para>
        ///   <para> -function(index, currentClass): A function returning one or more space-separated class names to be added to the existing class name(s). Receives the index position of the element in the set and the existing class name(s) as arguments. Within the function, this refers to the current element in the set. </para>
        /// </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector AddClass(string className_function)
        {
            if (RequiresApostrophe(className_function))
            {
                ExecJs("", ".addClass('" + className_function + "')");
            }
            else
            {
                ExecJs("", ".addClass(" + className_function + ")");
            }
            return this;
        }


        /// <summary>
        ///   Insert content, specified by the parameter, after each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/after/ </para>
        /// </summary>
        /// <param name="content_content">
        ///   <para> -content: HTML string, DOM element, or jQuery object to insert after each element in the set of matched elements. </para>
        ///   <para> -content: One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert after each element in the set of matched elements. </para>
        /// </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector After(params string[] content_content)
        {
            string resultingContent = "";
            foreach (string s in content_content)
            {
                if (RequiresApostrophe(s))
                {
                    resultingContent += "'" + s + "',";
                }
                else
                {
                    resultingContent += s + ",";
                }
            }
            resultingContent = resultingContent.Remove(resultingContent.Length - 1);
            object result = ExecJs("", ".after(" + resultingContent + ")");
            _subset = ObjectToJQueryTagList(result);
            return this;
        }


        /// <summary>
        ///   Insert content, specified by the parameter, to the end of each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/append/ </para>
        /// </summary>
        /// <param name="content_content">
        ///   <para> -content: DOM element, HTML string, or jQuery object to insert at the end of each element in the set of matched elements. </para>
        ///   <para> -content: One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert at the end of each element in the set of matched elements. </para>
        /// </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector Append(params string[] content_content)
        {
            string resultingContent = string.Join(",", content_content.Select(Fix));
            object result = ExecJs("", ".append(" + resultingContent + ")");
            _subset = ObjectToJQueryTagList(result);
            return this;
        }


        /// <summary>
        ///   Insert every element in the set of matched elements to the end of the target.
        ///   <para> Source: http://api.jquery.com/appendTo/ </para>
        /// </summary>
        /// <param name="target"> A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted at the end of the element(s) specified by this parameter. </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector AppendTo(string target)
        {
            if (RequiresApostrophe(target))
            {
                ExecJs("", ".appendTo('" + target + "')");
            }
            else
            {
                ExecJs("", ".appendTo(" + target + ")");
            }
            return this;
        }


        /// <summary>
        ///   Get the value of an attribute for the first element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/attr/#attr1 </para>
        /// </summary>
        /// <param name="attribute_name"> The name of the attribute to get. </param>
        /// <returns> String containing the element's attribute value. </returns>
        public string Attr(string attribute_name)
        {
            return _subset[0].WebElement.GetAttribute(attribute_name);
        }

        /// <summary>
        ///   Set one or more attributes for the set of matched elements.
        ///   <para> Source: http://api.jquery.com/attr/#attr2 </para>
        /// </summary>
        /// <param name="attribute_name"> The name of the attribute to set. </param>
        /// <param name="new_value"> A value to set for the attribute. </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector Attr(string attribute_name, string new_value)
        {
            if (RequiresApostrophe(new_value))
            {
                ExecJs("", ".attr(\"" + attribute_name + "\",'" + new_value + "')");
            }
            else
            {
                ExecJs("", ".attr(\"" + attribute_name + "\"," + new_value + ")");
            }
            return this;
        }

        /// <summary>
        ///   Insert content, specified by the parameter, before each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/before/ </para>
        /// </summary>
        /// <param name="content_content">
        ///   <para> -content: DOM element, HTML string, or jQuery object to insert at the end of each element in the set of matched elements. </para>
        ///   <para> -content: One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert at the end of each element in the set of matched elements. </para>
        /// </param>
        /// <returns> </returns>
        public JQuerySelector Before(params string[] content_content)
        {
            string resultingContent = "";
            foreach (string s in content_content)
            {
                if (RequiresApostrophe(s))
                {
                    resultingContent += "'" + s + "',";
                }
                else
                {
                    resultingContent += s + ",";
                }
            }
            resultingContent = resultingContent.Remove(resultingContent.Length - 1);
            Console.WriteLine(resultingContent);
            object result = ExecJs("", ".before(" + resultingContent + ")");
            _subset = ObjectToJQueryTagList(result);
            return this;
        }

        /// <summary>
        ///   Bind an event handler to the "click" JavaScript event, or trigger that event on an element.
        /// </summary>
        public void Click()
        {
            ExecJs("", ".click()");
        }


        /// <summary>
        ///   Get the value of a style property for the first element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/css/#css1 </para>
        /// </summary>
        /// <param name="css_property"> A CSS property. </param>
        /// <returns> String containing the CSS property value. </returns>
        public string Css(string css_property)
        {
            return _subset[0].Css(css_property);
        }

        /// <summary>
        ///   Set one or more CSS properties for the set of matched elements.
        ///   <para> Source: http://api.jquery.com/css/#css2 </para>
        /// </summary>
        /// <param name="css_property"> A CSS property name. </param>
        /// <param name="new_value"> A value to set for the property. </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector Css(string css_property, string new_value)
        {
            if (RequiresApostrophe(new_value))
            {
                ExecJs("", ".css(\"" + css_property + "\",'" + new_value + "')");
            }
            else
            {
                ExecJs("", ".css(\"" + css_property + "\"," + new_value + ")");
            }
            return this;
        }


        ///<summary>
        ///  Executes a javascript function by concatenating a prefix and a suffix to the selector of the JQuerySelector.
        ///</summary>
        ///<param name="prefix"> It represents all the javascript code that goes before the selector. </param>
        ///<param name="suffix"> It represents all the javascript code that goes after the selector. </param>
        Object ExecJs(string prefix, string suffix)
        {
            return JQuery.JavaScriptExecutor.ExecuteScript("return " + prefix + _selector + suffix);
        }

        /// <summary>
        ///   Get the descendants of each element in the current set of matched elements, filtered by a selector, jQuery 
        ///   object, or element.
        ///   <para> Source: http://api.jquery.com/find/ </para>
        /// </summary>
        /// <param name="selector"> A string containing a selector expression to match elements against. </param>
        /// <returns> A JQuerySelector containing the descedants filtered by the selector. </returns>
        public JQuerySelector Find(string selector)
        {
            Object result = ExecJs("", ".find('" + selector + "')");
            return new JQuerySelector(_selector + ".find('" + selector + "')", ObjectToWebElementList(result));
        }

        /// <summary>
        ///   Reduce the set of matched elements to the first in the set.
        /// </summary>
        /// <returns> A JQueryTag containing the first element of the set of elements. </returns>
        public JQueryTag First()
        {
            Object result = ExecJs("", ".first()");
            return new JQueryTag(_selector, 0, ObjectToWebElementList(result)[0]);
        }

        /// <summary>
        ///   Determine whether any of the matched elements are assigned the given class.
        ///   <para> Source: http://api.jquery.com/hasClass/ </para>
        /// </summary>
        /// <param name="className"> The class name to search for. </param>
        /// <returns> True if anly element has the provided className.
        ///   <para> False if none of them has the provided className. </para>
        /// </returns>
        public bool HasClass(string className)
        {
            return _subset.Any(element => element.WebElement.GetAttribute("class").Contains(className));
        }

        /// <summary>
        ///   Determines if the JQuerySelector contains any elements
        /// </summary>
        /// <param name="comparer">The jQuerySelector to compare with</param>
        /// <returns> True if both jQuerySelectors have the same elements.
        ///   <para> False they don't. </para>
        /// </returns>
        public bool HasSameElementsOf(JQuerySelector comparer)
        {
            for (int i = 0; i < _subset.Count; i++)
            {
                try
                {
                    if (!_subset[i].Equals(comparer._subset[i]))
                        return false;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        ///   Get the HTML contents of the first element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/html/#html1 </para>
        /// </summary>
        /// <returns> A string containing the HTML contents of the first element in the set of matched elements. </returns>
        public string Html()
        {
            return ExecJs("", ".html()").ToString();
        }

        /// <summary>
        ///   Set the HTML contents of each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/html/#html2 </para>
        /// </summary>
        /// <param name="htmlString"> A string of HTML to set as the content of each matched element. </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector Html(string htmlString)
        {
            object result = ExecJs("", ".html('" + htmlString + "')");
            _subset = ObjectToJQueryTagList(result);
            return this;
        }

        /// <summary>
        ///   Determines if the JQuerySelector contains any elements
        /// </summary>
        /// <returns> True if it does not contain any elements.
        ///   <para> False if it contains elements. </para>
        /// </returns>
        public bool IsEmpty()
        {
            return !_subset.Any();
        }

        /// <summary>
        ///   Reduce the set of matched elements to the final one in the set.
        /// </summary>
        /// <returns> JQueryTag containing the last element of the set of elements. </returns>
        public JQueryTag Last()
        {
            Object result = ExecJs("", ".last()");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQueryTag(_selector, _subset.Count - 1, webElements[0]);
        }


        /// <summary>
        ///   Converts an object returned from a javaScript function into a list of JQueryTags
        /// </summary>
        /// <param name="result"> The result of the javaScript code. </param>
        /// <returns> A list containing all JQueryTags </returns>
        List<JQueryTag> ObjectToJQueryTagList(Object result)
        {
            var jQueryTags = new List<JQueryTag>();
            if (result is Dictionary<string, Object>)
            {
                var dictionary = (Dictionary<string, Object>) result;
                int length = Convert.ToInt32(dictionary["length"]);
                for (int i = 0; i < length; i++)
                {
                    jQueryTags.Add(new JQueryTag(_selector, i, (IWebElement) dictionary[Convert.ToString(i)]));
                }
            }
            else
            {
                var webElements = new List<IWebElement>(((ReadOnlyCollection<IWebElement>) result));
                for (int i = 0; i < webElements.Count; i++)
                {
                    jQueryTags.Add(new JQueryTag(_selector, i, webElements[i]));
                }
            }
            return jQueryTags;
        }

        /// <summary>
        ///   Converts an object into a list of web elements.
        /// </summary>
        /// <param name="result"> The object returned from a javascript execution. </param>
        /// <returns> A list of web elements. </returns>
        static List<IWebElement> ObjectToWebElementList(Object result)
        {
            var webElements = new List<IWebElement>();
            if (result is Dictionary<string, Object>)
            {
                var dictionary = (Dictionary<string, Object>) result;
                int length = Convert.ToInt32(dictionary["length"]);
                for (int i = 0; i < length; i++)
                {
                    webElements.Add((IWebElement) dictionary[Convert.ToString(i)]);
                }
            }
            else
            {
                webElements = new List<IWebElement>(((ReadOnlyCollection<IWebElement>) result));
            }
            return webElements;
        }


        /// <summary>
        ///   Overwrites all of the selectors of each of the JQueryTags and the JQuerySelector
        /// </summary>
        /// <param name="selector"> The new selector for the JQuerySelector and its JQueryTags </param>
        public void OverwriteSelectors(string selector)
        {
            _selector = selector;
            for (int i = 0; i < _subset.Count; i++)
            {
                _subset[i].Selector = selector + "[" + i + "]";
            }
        }

        /// <summary>
        ///   Get the parent of each element in the current set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <returns> A JQuerySelector containing the parent of each element in the current set of elements. </returns>
        public JQuerySelector Parent()
        {
            Object result = ExecJs("", ".parent()");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQuerySelector(_selector + ".parent()", webElements);
        }


        /// <summary>
        ///   Remove the set of matched elements from the DOM.
        ///   <para> Source: http://api.jquery.com/remove/ </para>
        /// </summary>
        public void Remove()
        {
            ExecJs("", ".remove()");
        }

        /// <summary>
        ///   Remove the set of matched elements from the DOM.
        ///   <para> Source: http://api.jquery.com/remove/ </para>
        /// </summary>
        /// <param name="selector"> A selector expression that filters the set of matched elements to be removed. </param>
        public void Remove(string selector)
        {
            ExecJs("", ".remove('" + selector + "')");
        }

        /// <summary>
        ///   Remove a single class, multiple classes, or all classes from each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/removeClass/ </para>
        /// </summary>
        public void RemoveClass()
        {
            ExecJs("", ".removeClass()");
        }

        /// <summary>
        ///   Remove a single class, multiple classes, or all classes from each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/removeClass/ </para>
        /// </summary>
        /// <param name="className"> One or more space-separated classes to be removed from the class attribute of each matched element. </param>
        public void RemoveClass(string className)
        {
            ExecJs("", ".removeClass('" + className + "')");
        }


        /// <summary>
        ///   Determines if a parameter of a javaScript function requires apostrophes around it.
        /// </summary>
        /// <param name="parameter"> The parameter of a javaScript function </param>
        /// <returns> True if it requires to be wrapped in apostrophes.
        ///   <para> False if it doesn't require to be wrapped in apostrophes. </para>
        /// </returns>
        static bool RequiresApostrophe(string parameter)
        {
            if (parameter.Split('(')[0].Contains("function") || parameter.Split('.')[0].Contains("document")
                || parameter.Split('(')[0].Contains("$") || parameter.Split('(')[0].Contains("jQuery"))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        ///   Get the combined text contents of each element in the set of matched elements, including their descendants.
        ///   <para> Source: http://api.jquery.com/text/#text1 </para>
        /// </summary>
        /// <returns> A string containing the text of an HTML element. </returns>
        public string Text()
        {
            object result = ExecJs("", ".text()");
            string resultText = result.ToString();
            return resultText;
        }

        /// <summary>
        ///   Set the content of each element in the set of matched elements to the specified text.
        ///   <para> Source: http://api.jquery.com/text/#text2 </para>
        /// </summary>
        /// <param name="textString_function">
        ///   <para> textString A string of text to set as the content of each matched element. </para>
        ///   <para> function(index, text) A function returning the text content to set. Receives the index position of the element in the set and the old text value as arguments. </para>
        /// </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector Text(string textString_function)
        {
            Object result;
            if (RequiresApostrophe(textString_function))
            {
                result = ExecJs("", ".text('" + textString_function + "')");
            }
            else
            {
                result = ExecJs("", ".text(" + textString_function + ")");
            }
            _subset = ObjectToJQueryTagList(result);
            return this;
        }

        /// <summary>
        ///   Get the current value of the first element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/val/#val1 </para>
        /// </summary>
        /// <returns> A string containing the current value of the first element in the set of matched elements. </returns>
        public string Val()
        {
            return ExecJs("", ".val()").ToString();
        }

        /// <summary>
        ///   Set the value of each element in the set of matched elements.
        ///   <para> Source: http://api.jquery.com/val/#val2 </para>
        /// </summary>
        /// <param name="value"> A string of text or an array of strings corresponding to the value of each matched element to set as selected/checked. </param>
        /// <returns> JQuerySelector containing the modified elements. </returns>
        public JQuerySelector Val(string value)
        {
            object result = ExecJs("", ".val('" + value + "')");
            _subset = ObjectToJQueryTagList(result);
            return this;
        }

        /// <summary>
        /// Get the immediately following sibling of each element in the set of matched elements. 
        /// </summary>
        /// <returns>A JQuerySelector containing the following sibling of each element in the set of elements.</returns>
        public JQuerySelector Next()
        {
            object result = ExecJs("", ".next()");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQuerySelector(_selector + ".next()", webElements);
        }

        /// <summary>
        /// Get the immediately following sibling of each element in the set of matched elements if they match the selector provided.
        /// </summary>
        /// <returns>A JQuerySelector containing the following sibling of each element in the set of elements.</returns>
        public JQuerySelector Next(string selector)
        {
            object result = ExecJs("", ".next(" + selector + ")");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQuerySelector(selector + ".next()", webElements);
        }


        /// <summary>
        /// Given a jQuery object that represents a set of DOM elements, the .nextAll() method allows us to search through the successors 
        /// of these elements in the DOM tree and construct a new jQuery object from the matching elements.
        /// </summary>
        /// <returns>A JQuerySelector containing the matching elements.</returns>
        public JQuerySelector NextAll()
        {
            object preResult = ExecJs("", ".nextAll()");
            List<IWebElement> webElements = ObjectToWebElementList(preResult);
            return new JQuerySelector(_selector + ".nextAll()", webElements);
        }

        /// <summary>
        /// Get all preceding siblings of each element in the set of matched elements.
        /// </summary>
        /// <returns> A JQuerySelector with all the previous elements </returns>
        public JQuerySelector Prev()
        {
            object result = ExecJs("", ".prev()");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQuerySelector(_selector + ".prev()", webElements);
        }

        /// <summary>
        /// Get all preceding siblings of each element in the set of matched elements filtered by a selector
        /// </summary>
        /// <returns> A JQuerySelector with all the previous elements </returns>
        public JQuerySelector Prev(string selector)
        {
            object result = ExecJs("", ".prev(" + selector + ")");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQuerySelector(selector + ".prev()", webElements);
        }

        /// <summary>
        /// Get all the previous elements of a specific jQuerySelector 
        /// </summary>
        /// <returns> A JQuerySelector with all the previous elements </returns>
        public JQuerySelector PrevAll()
        {
            object preResult = ExecJs("", ".prevAll()");
            List<IWebElement> webElements = ObjectToWebElementList(preResult);
            return new JQuerySelector(_selector + ".prevAll()", webElements);
        }


        /// <summary>
        /// Given a jQuery object that represents a set of DOM elements, the .summary() method allows us see the elements and their tags to see
        /// wich elements we have in the selector
        /// </summary>
        /// <returns>A String with the structure tag: text of all the elements within the selector.</returns>
        public string Summary()
        {
            string result = "";
            foreach (JQueryTag element in _subset)
            {
                result += element.TagName + ": " + element.Text() + "\n";
            }
            return result;
        }

        /// <summary>
        ///   Add the previous set of elements on the stack to the current set.
        /// </summary>
        /// <returns> jQuerySelector containing the previous set elements and the current one </returns>
        public JQuerySelector AndSelf()
        {
            object result = ExecJs("", ".andSelf()");
            List<IWebElement> webElements = ObjectToWebElementList(result);
            return new JQuerySelector(_selector + ".andSelf()", webElements);
        }

        /// <summary>
        ///   Get the children of each element in the set of matched elements.
        /// </summary>
        /// <returns> jQuerySelector containing the children of the current set of elements</returns>
        public JQuerySelector Children()
        {
            object preResult = ExecJs("", ".children()");
            List<IWebElement> webElements = ObjectToWebElementList(preResult);
            return new JQuerySelector(_selector + ".children()", webElements);
        }

        /// <summary>
        ///   Get the children of each element in the set of matched elements filtered by a selector
        /// </summary>
        /// <returns> jQuerySelector containing the children of the specified set of elements</returns>
        public JQuerySelector Children(string selector)
        {
            object preResult = ExecJs("", ".children(" + selector + ")");
            List<IWebElement> webElements = ObjectToWebElementList(preResult);
            return new JQuerySelector(selector + ".children()", webElements);
        }


        /// <summary>
        ///   Get the selector of the current jQuerySelector
        /// </summary>
        /// <returns> a string representing the selector of the jQuerySelector </returns>
        public string GetSelector()
        {
            return _selector;
        }
    }
}