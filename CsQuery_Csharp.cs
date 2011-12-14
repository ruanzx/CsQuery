﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Jtc.CsQuery.Utility;
using Jtc.CsQuery.Engine;
using Jtc.CsQuery.ExtensionMethods;

namespace Jtc.CsQuery
{
    /// <summary>
    /// This partial contains properties & methods that are specific to the C# implementation and not part of jQuery
    /// </summary>
    public partial class CsQuery
    {
        #region private properties
        protected SelectorChain _Selectors = null;
        protected IDomRoot _Document = null;
        #endregion
        
        #region public properties
      
        /// <summary>
        /// Represents the full, parsed DOM for an object created with an HTML parameter
        /// </summary>
        public IDomRoot Document
        {
            get
            {
                if (_Document == null)
                {
                    CreateNewDom();
                }
                return _Document;
            }
            protected set
            {
                _Document = value;
            }
        } 
        
        /// <summary>
        ///  The selector (parsed) used to create this instance
        /// </summary>
        public SelectorChain Selectors
        {
            get
            {
                return _Selectors;
            }
            protected set
            {
                _Selectors = value;
            }
        }


        /// <summary>
        /// The entire selection set as an enumerable, same as enumerting on the object itself (though this may
        /// allow you to more easily use extension methods)
        /// </summary>
        public IEnumerable<IDomObject> Selection
        {
            get
            {
                return SelectionSet;
            }
        }

        /// <summary>
        /// Returns just IDomElements from the selection list.
        /// </summary>
        public IEnumerable<IDomElement> Elements
        {
            get
            {
                return onlyElements(SelectionSet);
            }
        }

        public SelectionSetOrder Order
        {
            get
            {
                return SelectionSet.Order;
            }
            set
            {
                SelectionSet.Order = value;
            }
        }

        /// <summary>
        /// Returns the HTML for all selected documents, separated by commas. No inner html or children are included.
        /// </summary>
        /// 
        public string SelectionHtml()
        {
            return SelectionHtml(false);
        }

        public string SelectionHtml(bool includeInner)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IDomObject elm in this)
            {

                sb.Append(sb.Length == 0 ? String.Empty : ", ");
                sb.Append(includeInner ? elm.Render() : elm.ToString());
            }
            return sb.ToString();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Renders just the selection set completely.
        /// </summary>
        /// <returns></returns>
        public string RenderSelection()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IDomObject elm in this)
            {
                sb.Append(elm.Render());
            }
            return sb.ToString();
        }
        /// <summary>
        /// Renders the DOM to a string
        /// </summary>
        /// <returns></returns>
        public string Render()
        {
            return Document.Render();
        }
        /// <summary>
        /// Render the complete DOM with specific options
        /// </summary>
        /// <param name="renderingOptions"></param>
        /// <returns></returns>
        public string Render(DomRenderingOptions renderingOptions)
        {
            Document.DomRenderingOptions = renderingOptions;
            return Render();
        }
        /// <summary>
        /// Returns a new empty CsQuery object bound to this domain
        /// </summary>
        /// <returns></returns>
        public CsQuery New()
        {
            CsQuery csq = new CsQuery();
            csq.CsQueryParent = this;
            return csq;
        }
        /// <summary>
        /// Returns a new empty CsQuery object bound to this domain, whose results are returned in the specified order
        /// </summary>
        /// <returns></returns>
        public CsQuery New(SelectionSetOrder order)
        {
            CsQuery csq = new CsQuery();
            csq.CsQueryParent = this;
            csq.Order = order;
            return csq;
        }
        /// <summary>
        /// Return a CsQuery object wrapping the enumerable passed, or the object itself if 
        /// already a CsQuery obect. Unlike CsQuery(context), this will not create a new CsQuery object from 
        /// an existing one.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public CsQuery EnsureCsQuery(IEnumerable<IDomObject> elements)
        {
            return elements is CsQuery ? (CsQuery)elements : new CsQuery(elements);
        }
        /// <summary>
        /// The first IDomElement (e.g. not text/special nodes) in the selection set, or null if none
        /// </summary>
        public IDomElement FirstElement()
        {

            using (IEnumerator<IDomElement> enumer = Elements.GetEnumerator())
            {
                if (enumer.MoveNext())
                {
                    return enumer.Current;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Removes one of two selectors/objects based on the value of the first parameter. The remaining one is
        /// explicitly shown. True keeps the first, false keeps the 2nd.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public CsQuery KeepOne(bool which, string trueSelector, string falseSelector)
        {
            return KeepOne(which ? 0 : 1, trueSelector, falseSelector);
        }
        public CsQuery KeepOne(bool which, CsQuery trueContent, CsQuery falseContent)
        {
            return KeepOne(which ? 0 : 1, trueContent, falseContent);
        }
        /// <summary>
        /// Removes all but one of a list selectors/objects based on the value of the first parameter. The remaining one is
        /// explicitly shown. The value of which is zero-based.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public CsQuery KeepOne(int which, params string[] content)
        {
            CsQuery[] arr = new CsQuery[content.Length];
            for (int i = 0; i < content.Length; i++)
            {
                arr[i] = Select(content[i]);
            }
            return KeepOne(which, arr);
        }
        public CsQuery KeepOne(int which, params CsQuery[] content)
        {
            for (int i = 0; i < content.Length; i++)
            {
                if (i == which)
                {
                    content[i].Show();
                }
                else
                {
                    content[i].Remove();
                }
            }
            return this;
        }
        /// <summary>
        /// Conditionally includes a selection. This is the equivalent of calling Remove() only when "include" is false
        /// (extension of jQuery API)
        /// </summary>
        /// <returns></returns>
        public CsQuery IncludeWhen(bool include)
        {
            if (!include)
            {
                Remove();
            }
            return this;
        }

        public CsQuery SetSelected(string groupName, IConvertible value)
        {
            var group = this.Find("input[name='" + groupName + "'][value='" + value + "']");
            if (group.Length == 0)
            {
                group = this.Find("#" + groupName);
            }
            if (group.Length > 0)
            {
                string nodeName = group[0].NodeName;
                string type = group[0]["type"];
                if (nodeName == "option" || 
                    (nodeName == "input" && (type=="radio" || type=="checkbox")))
                {
                    group.Prop("checked", true);
                }
            }
            return this;
        }

        /// <summary>
        /// The current selection set will become the DOM. This is destructive.
        /// </summary>
        /// <returns></returns>
        public CsQuery MakeRoot()
        {
            Document.ChildNodes.Clear();
            Document.ChildNodes.AddRange(Elements);
            return this;
        }
        public CsQuery MakeRoot(string selector)
        {
            return Select(selector).MakeRoot();
        }
        public override string ToString()
        {
            return SelectionHtml();
        }

        #endregion

        #region interface members

        public IEnumerator<IDomObject> GetEnumerator()
        {
            return SelectionSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SelectionSet.GetEnumerator();
        }

        #endregion

    }
}