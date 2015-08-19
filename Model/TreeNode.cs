using System;
using System.Collections.Generic;
using System.Linq;

namespace GloryS.Common.Model
{
    public class TreeNode<TElement>
    {
        #region Constructors

        public TreeNode()
        {
            Nodes = new List<TreeNode<TElement>>();
        }

        public TreeNode(TElement element, IEnumerable<TreeNode<TElement>> nodes = null)
        {
            Element = element;
            nodes = nodes ?? new List<TreeNode<TElement>>();
            Nodes = nodes;
        }

        #endregion

        #region Properties

        public TElement Element { get; set; }

        public IEnumerable<TreeNode<TElement>> Nodes { get; set; }

        #endregion

        #region Static methods

        public static IEnumerable<TreeNode<TElement>> CreateBranch<TModel>(IEnumerable<TModel> targetModels, Func<TModel, TElement> nodeSelector, Func<TModel, IEnumerable<TModel>> subElementsSelector)
        {
            return targetModels.Select(m => CreateTree(m, nodeSelector, subElementsSelector));
        }

        private static TreeNode<TElement> CreateTree<TModel>(TModel targetModel, Func<TModel, TElement> nodeSelector, Func<TModel, IEnumerable<TModel>> subElementsSelector, int maxDepth, int depth)
        {
            var subItems = subElementsSelector(targetModel);

            IEnumerable<TreeNode<TElement>> nodes = null;

            if (maxDepth>depth && subItems != null && (subItems = subItems.ToList()).Any())
            {
                nodes = subItems.Select(item => CreateTree(item, nodeSelector, subElementsSelector, maxDepth, depth + 1));
            }

            return new TreeNode<TElement>(nodeSelector(targetModel), nodes);
        }

        public static TreeNode<TElement> CreateTree<TModel>(TModel targetModel, Func<TModel, TElement> nodeSelector, Func<TModel, IEnumerable<TModel>> subElementsSelector, int depth = int.MaxValue)
        {
            return CreateTree(targetModel, nodeSelector, subElementsSelector, depth, 0);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Casts current Tree to List of Elements of All Nodes.
        /// </summary>
        /// <returns>List of Elements</returns>
        public List<TElement> ElementsToList()
        {
            var elements = new List<TElement>();

            AddToList(elements);

            return elements;
        }

        private void AddToList(List<TElement> elements)
        {
            elements.Add(Element);

            if (Nodes != null)
            {
                foreach (var node in Nodes)
                {
                    node.AddToList(elements);
                }
            }
        }

        #endregion

        public TreeNode<TNewElement> CastTree<TNewElement>(Func<TElement, TNewElement> castMethod)
        {
            return new TreeNode<TNewElement>(castMethod(Element), Nodes == null ? null : Nodes.Select(n => n.CastTree(castMethod)));
        }
    }
}
