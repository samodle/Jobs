using Analytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HTMLDev
{
    public static class NetworkHTML
    {
        public const string quote = "\"";
        public const string quote_single = "\'";


        public static void writeGraphHTML(ONETReport forkReport, string FileName, string FileType = ".html")
        {
            string a = "";
            Random random = new Random();
            string fileName = Windows_Desktop.Publics.FILEPATHS.PATH_FORK_HTML + FileName + FileType;

            FileStream fcreate = File.Open(fileName, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fcreate))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<meta charset = " + quote + "utf-8" + quote + ">");
                writer.WriteLine("<svg width = " + quote + "100%" + quote + " height = " + quote + "100%" + quote + "></svg>");
                writer.WriteLine("<script src = " + quote + "https://d3js.org/d3.v4.min.js" + quote + "></script>");
                writer.WriteLine("<script>");
                writer.WriteLine("var baseNodes = [");

                for (int i = 0; i < forkReport.MasterOccupationList.Count - 1; i++)
                {
                    writer.WriteLine("{id: " + quote + forkReport.MasterOccupationList[i].Name.Replace(" ", "") + quote + ", group: 0, label: " + quote + forkReport.MasterOccupationList[i].Name + quote + ", level: 1},");
                }
                int j = forkReport.MasterOccupationList.Count - 1;
                writer.WriteLine("{id: " + quote + forkReport.MasterOccupationList[j].Name.Replace(" ", "") + quote + ", group: 0, label: " + quote + forkReport.MasterOccupationList[j].Name + quote + ", level: 1}");
                /*
                 writer.WriteLine("{id: " + quote + "mammal" + quote + ", group: 0, label: " + quote + "Mammals" + quote + ", level: 1},"); 
                 writer.WriteLine("{id: " + quote + "dog" + quote + "   , group: 0, label: " + quote + "Dogs" + quote + "   , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "cat" + quote + "   , group: 0, label: " + quote + "Cats" + quote + "   , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "fox" + quote + "   , group: 0, label: " + quote + "Foxes" + quote + "  , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "elk" + quote + "   , group: 0, label: " + quote + "Elk" + quote + "    , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "insect" + quote + ", group: 1, label: " + quote + "Insects" + quote + ", level: 1},"); 
                 writer.WriteLine("{id: " + quote + "ant" + quote + "   , group: 1, label: " + quote + "Ants" + quote + "   , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "bee" + quote + "   , group: 1, label: " + quote + "Bees" + quote + "   , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "fish" + quote + "  , group: 2, label: " + quote + "Fish" + quote + "   , level: 1},"); 
                 writer.WriteLine("{id: " + quote + "carp" + quote + "  , group: 2, label: " + quote + "Carp" + quote + "   , level: 2},"); 
                 writer.WriteLine("{id: " + quote + "pike" + quote + "  , group: 2, label: " + quote + "Pikes" + quote + "  , level: 2}"); 
                 */
                writer.WriteLine("]");
                writer.WriteLine("var baseLinks = [");
                for (int i = 0; i < forkReport.MasterOccupationList.Count - 1; i++)
                {
                    if (i < forkReport.MasterOccupationList.Count / 2)
                    {
                        writer.WriteLine("{target: " + quote + forkReport.MasterOccupationList[i + 1].Name.Replace(" ", "") + quote + ", source: " + quote + forkReport.MasterOccupationList[i].Name.Replace(" ", "") + quote + " , strength: " + random.NextDouble() + "},");
                        writer.WriteLine("{target: " + quote + forkReport.MasterOccupationList[i + 2].Name.Replace(" ", "") + quote + ", source: " + quote + forkReport.MasterOccupationList[i].Name.Replace(" ", "") + quote + " , strength: " + random.NextDouble() + "},");
                    }
                    else
                    {
                        writer.WriteLine("{target: " + quote + forkReport.MasterOccupationList[i - 1].Name.Replace(" ", "") + quote + ", source: " + quote + forkReport.MasterOccupationList[i].Name.Replace(" ", "") + quote + " , strength: " + random.NextDouble() + "},");
                        writer.WriteLine("{target: " + quote + forkReport.MasterOccupationList[i - 2].Name.Replace(" ", "") + quote + ", source: " + quote + forkReport.MasterOccupationList[i].Name.Replace(" ", "") + quote + " , strength: " + random.NextDouble() + "},");
                    }
                }
                writer.WriteLine("{target: " + quote + forkReport.MasterOccupationList[j - 1].Name.Replace(" ", "") + quote + ", source: " + quote + forkReport.MasterOccupationList[j].Name.Replace(" ", "") + quote + " , strength: " + random.NextDouble() + "}");
                /*
                 writer.WriteLine("{target: " + quote + "mammal" + quote + ", source: " + quote + "dog" + quote + " , strength: 0.7},"); 
                 writer.WriteLine("	{target: " + quote + "mammal" + quote + ", source: " + quote + "cat" + quote + " , strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "mammal" + quote + ", source: " + quote + "fox" + quote + " , strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "mammal" + quote + ", source: " + quote + "elk" + quote + " , strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "insect" + quote + ", source: " + quote + "ant" + quote + " , strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "insect" + quote + ", source: " + quote + "bee" + quote + " , strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "fish" + quote + "  , source: " + quote + "carp" + quote + ", strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "fish" + quote + "  , source: " + quote + "pike" + quote + ", strength: 0.7},"); 
                 writer.WriteLine("{target: " + quote + "cat" + quote + "   , source: " + quote + "elk" + quote + " , strength: 0.1},"); 
                 writer.WriteLine("{target: " + quote + "carp" + quote + "  , source: " + quote + "ant" + quote + " , strength: 0.1},"); 
                 writer.WriteLine("{target: " + quote + "elk" + quote + "   , source: " + quote + "bee" + quote + " , strength: 0.1},"); 
                 writer.WriteLine("{target: " + quote + "dog" + quote + "   , source: " + quote + "cat" + quote + " , strength: 0.1},"); 
                 writer.WriteLine("{target: " + quote + "fox" + quote + "   , source: " + quote + "ant" + quote + " , strength: 0.1},"); 
                 writer.WriteLine("	{target: " + quote + "pike" + quote + "  , source: " + quote + "cat" + quote + " , strength: 0.1}"); 
                 */

                writer.WriteLine("]");
                writer.WriteLine("var nodes = [...baseNodes]");
                writer.WriteLine("var links = [...baseLinks]");
                writer.WriteLine("function getNeighbors(node)");
                writer.WriteLine("{");
                writer.WriteLine("return baseLinks.reduce(function(neighbors, link) {");
                writer.WriteLine("if (link.target.id === node.id)");
                writer.WriteLine("{");
                writer.WriteLine("neighbors.push(link.source.id)");
                writer.WriteLine("}");
                writer.WriteLine("else if (link.source.id === node.id)");
                writer.WriteLine("{");
                writer.WriteLine("neighbors.push(link.target.id)");
                writer.WriteLine("}");
                writer.WriteLine("return neighbors");
                writer.WriteLine("},");
                writer.WriteLine("[node.id]");
                writer.WriteLine(")");
                writer.WriteLine("}");
                writer.WriteLine("function isNeighborLink(node, link)");
                writer.WriteLine("{");
                writer.WriteLine("return link.target.id === node.id || link.source.id === node.id");
                writer.WriteLine("}");
                writer.WriteLine("function getNodeColor(node, neighbors)");
                writer.WriteLine("{");
                writer.WriteLine("if (Array.isArray(neighbors) && neighbors.indexOf(node.id)> -1)");
                writer.WriteLine("{");
                writer.WriteLine("return node.level === 1 ? " + quote_single + "blue" + quote_single + " : " + quote_single + "green" + quote_single + "");
                writer.WriteLine("}");
                writer.WriteLine("return node.level === 1 ? " + quote_single + "red" + quote_single + " : " + quote_single + "gray" + quote_single + "");
                writer.WriteLine("}");
                writer.WriteLine("function getLinkColor(node, link)");
                writer.WriteLine("{");
                writer.WriteLine("return isNeighborLink(node, link) ? " + quote_single + "green" + quote_single + " : " + quote_single + "#E5E5E5" + quote_single + "");
                writer.WriteLine("}");
                writer.WriteLine("function getTextColor(node, neighbors)");
                writer.WriteLine("{");
                writer.WriteLine("return Array.isArray(neighbors) && neighbors.indexOf(node.id)> -1 ? " + quote_single + "green" + quote_single + " : " + quote_single + "black" + quote_single + "");
                writer.WriteLine("}");
                writer.WriteLine("var width = window.innerWidth");
                writer.WriteLine("var height = window.innerHeight");
                writer.WriteLine("var svg = d3.select(" + quote_single + "svg" + quote_single + ")");
                writer.WriteLine("svg.attr(" + quote_single + "width" + quote_single + ", width).attr(" + quote_single + "height" + quote_single + ", height)");
                writer.WriteLine("var linkElements,");
                writer.WriteLine("nodeElements,");
                writer.WriteLine("textElements");
                writer.WriteLine("var linkGroup = svg.append(" + quote_single + "g" + quote_single + ").attr(" + quote_single + "class" + quote_single + ", " + quote_single + "links" + quote_single + ")");
                writer.WriteLine("var nodeGroup = svg.append(" + quote_single + "g" + quote_single + ").attr(" + quote_single + "class" + quote_single + ", " + quote_single + "nodes" + quote_single + ")");
                writer.WriteLine("var textGroup = svg.append(" + quote_single + "g" + quote_single + ").attr(" + quote_single + "class" + quote_single + ", " + quote_single + "texts" + quote_single + ")");
                writer.WriteLine("var selectedId");
                writer.WriteLine("var linkForce = d3");
                writer.WriteLine(".forceLink()");
                writer.WriteLine(".id(function(link) {return link.id})");
                writer.WriteLine(".strength(function (link) {return link.strength})");
                writer.WriteLine("var simulation = d3");
                writer.WriteLine(".forceSimulation()");
                writer.WriteLine(".force(" + quote_single + "link" + quote_single + ", linkForce)");
                writer.WriteLine(".force(" + quote_single + "charge" + quote_single + ", d3.forceManyBody().strength(-120))");
                writer.WriteLine(".force(" + quote_single + "center" + quote_single + ", d3.forceCenter(width / 2, height / 2))");
                writer.WriteLine("var dragDrop = d3.drag().on(" + quote_single + "start" + quote_single + ", function(node) {");
                writer.WriteLine("node.fx = node.x");
                writer.WriteLine("node.fy = node.y");
                writer.WriteLine("}).on(" + quote_single + "drag" + quote_single + ", function (node) {");
                writer.WriteLine("simulation.alphaTarget(0.7).restart()");
                writer.WriteLine("node.fx = d3.event.x");
                writer.WriteLine("node.fy = d3.event.y");
                writer.WriteLine("}).on(" + quote_single + "end" + quote_single + ", function (node) {");
                writer.WriteLine("if (!d3.event.active) {");
                writer.WriteLine("simulation.alphaTarget(0)");
                writer.WriteLine("}");
                writer.WriteLine("node.fx = null");
                writer.WriteLine("node.fy = null");
                writer.WriteLine("})");
                writer.WriteLine("function selectNode(selectedNode)");
                writer.WriteLine("{");
                writer.WriteLine("if (selectedId === selectedNode.id)");
                writer.WriteLine("{");
                writer.WriteLine("selectedId = undefined");
                writer.WriteLine("resetData()");
                writer.WriteLine("updateSimulation()");
                writer.WriteLine("}");
                writer.WriteLine("else");
                writer.WriteLine("{");
                writer.WriteLine("selectedId = selectedNode.id");
                writer.WriteLine("updateData(selectedNode)");
                writer.WriteLine("updateSimulation()");
                writer.WriteLine("}");
                writer.WriteLine("var neighbors = getNeighbors(selectedNode)");
                writer.WriteLine("nodeElements.attr(" + quote_single + "fill" + quote_single + ", function(node) {return getNodeColor(node, neighbors)})");
                writer.WriteLine("textElements.attr(" + quote_single + "fill" + quote_single + ", function(node) {return getTextColor(node, neighbors)})");
                writer.WriteLine("linkElements.attr(" + quote_single + "stroke" + quote_single + ", function(link) {return getLinkColor(selectedNode, link)})");
                writer.WriteLine("}");
                writer.WriteLine("function resetData()");
                writer.WriteLine("{");
                writer.WriteLine("var nodeIds = nodes.map(function(node) {return node.id})");
                writer.WriteLine("baseNodes.forEach(function(node) {");
                writer.WriteLine("if (nodeIds.indexOf(node.id) === -1)");
                writer.WriteLine("{");
                writer.WriteLine("nodes.push(node)");
                writer.WriteLine("}");
                writer.WriteLine("})");
                writer.WriteLine("links = baseLinks");
                writer.WriteLine("}");
                writer.WriteLine("function updateData(selectedNode)");
                writer.WriteLine("{");
                writer.WriteLine("var neighbors = getNeighbors(selectedNode)");
                writer.WriteLine("var newNodes = baseNodes.filter(function(node) {");
                writer.WriteLine("return neighbors.indexOf(node.id)> -1 || node.level === 1");
                writer.WriteLine("})");
                writer.WriteLine("var diff = {");
                writer.WriteLine("removed: nodes.filter(function(node) {return newNodes.indexOf(node) === -1}),");
                writer.WriteLine("added: newNodes.filter(function(node) {return nodes.indexOf(node) === -1})");
                writer.WriteLine("}");
                writer.WriteLine("diff.removed.forEach(function (node) {nodes.splice(nodes.indexOf(node), 1)})");
                writer.WriteLine("diff.added.forEach(function (node) {nodes.push(node)})");
                writer.WriteLine("links = baseLinks.filter(function (link) {");
                writer.WriteLine("return link.target.id === selectedNode.id || link.source.id === selectedNode.id");
                writer.WriteLine("})");
                writer.WriteLine("}");
                writer.WriteLine("function updateGraph()");
                writer.WriteLine("{");
                writer.WriteLine("linkElements = linkGroup.selectAll(" + quote_single + "line" + quote_single + ")");
                writer.WriteLine(".data(links, function(link) {");
                writer.WriteLine("return link.target.id + link.source.id");
                writer.WriteLine("})");
                writer.WriteLine("linkElements.exit().remove()");
                writer.WriteLine("var linkEnter = linkElements");
                writer.WriteLine(".enter().append(" + quote_single + "line" + quote_single + ")");
                writer.WriteLine(".attr(" + quote_single + "stroke-width" + quote_single + ", 1)");
                writer.WriteLine(".attr(" + quote_single + "stroke" + quote_single + ", " + quote_single + "rgba(50, 50, 50, 0.2)" + quote_single + ")");
                writer.WriteLine("linkElements = linkEnter.merge(linkElements)");
                writer.WriteLine("nodeElements = nodeGroup.selectAll(" + quote_single + "circle" + quote_single + ")");
                writer.WriteLine(".data(nodes, function(node) {return node.id})");
                writer.WriteLine("nodeElements.exit().remove()");
                writer.WriteLine("var nodeEnter = nodeElements");
                writer.WriteLine(".enter()");
                writer.WriteLine(".append(" + quote_single + "circle" + quote_single + ")");
                writer.WriteLine(".attr(" + quote_single + "r" + quote_single + ", 10)");
                writer.WriteLine(".attr(" + quote_single + "fill" + quote_single + ", function(node) {return node.level === 1 ? " + quote_single + "red" + quote_single + " : " + quote_single + "gray" + quote_single + "})");
                writer.WriteLine(".call(dragDrop)");
                writer.WriteLine(".on(" + quote_single + "click" + quote_single + ", selectNode)");
                writer.WriteLine("nodeElements = nodeEnter.merge(nodeElements)");
                writer.WriteLine("textElements = textGroup.selectAll(" + quote_single + "text" + quote_single + ")");
                writer.WriteLine(".data(nodes, function(node) {return node.id})");
                writer.WriteLine("textElements.exit().remove()");
                writer.WriteLine("var textEnter = textElements");
                writer.WriteLine(".enter()");
                writer.WriteLine(".append(" + quote_single + "text" + quote_single + ")");
                writer.WriteLine(".text(function(node) {return node.label})");
                writer.WriteLine(".attr(" + quote_single + "font-size" + quote_single + ", 15)");
                writer.WriteLine(".attr(" + quote_single + "dx" + quote_single + ", 15)");
                writer.WriteLine(".attr(" + quote_single + "dy" + quote_single + ", 4)");
                writer.WriteLine("textElements = textEnter.merge(textElements)");
                writer.WriteLine("}");
                writer.WriteLine("function updateSimulation()");
                writer.WriteLine("{");
                writer.WriteLine("updateGraph()");
                writer.WriteLine("simulation.nodes(nodes).on(" + quote_single + "tick" + quote_single + ", () => {");
                writer.WriteLine("nodeElements");
                writer.WriteLine(".attr(" + quote_single + "cx" + quote_single + ", function(node) {return node.x})");
                writer.WriteLine(".attr(" + quote_single + "cy" + quote_single + ", function(node) {return node.y})");
                writer.WriteLine("textElements");
                writer.WriteLine(".attr(" + quote_single + "x" + quote_single + ", function(node) {return node.x})");
                writer.WriteLine(".attr(" + quote_single + "y" + quote_single + ", function(node) {return node.y})");
                writer.WriteLine("linkElements");
                writer.WriteLine(".attr(" + quote_single + "x1" + quote_single + ", function(link) {return link.source.x})");
                writer.WriteLine(".attr(" + quote_single + "y1" + quote_single + ", function(link) {return link.source.y})");
                writer.WriteLine(".attr(" + quote_single + "x2" + quote_single + ", function(link) {return link.target.x})");
                writer.WriteLine(".attr(" + quote_single + "y2" + quote_single + ", function(link) {return link.target.y})");
                writer.WriteLine("})");
                writer.WriteLine("simulation.force(" + quote_single + "link" + quote_single + ").links(links)");
                writer.WriteLine("simulation.alphaTarget(0.7).restart()");
                writer.WriteLine("}");
                writer.WriteLine("updateSimulation()");
                writer.WriteLine("</script>");

                writer.Close();
            }
        }
    }
}
