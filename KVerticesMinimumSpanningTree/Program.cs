using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using static KVerticesMinimumSpanningTree.Graph;

namespace KVerticesMinimumSpanningTree
{
    public class Graph
    {
        public int V, E;

        public Edge[] edgeList;

        public Verticle[] verticleList;

        public Graph(int v, int e)
        {
            V = v;
            E = e;
            verticleList = new Verticle[V];
            edgeList = new Edge[E];  
            for (int i = 0; i < e; ++i)
                edgeList[i] = new Edge();
        }

        public class Verticle
        {
            public int x_coord, y_coord;
        }

        public class Edge : IComparable<Edge>
        {
            public int src, dest, weight;

            public int CompareTo(Edge compareEdge)
            {
                return weight - compareEdge.weight;
            }
        }

        // Класс для представления подмножества для объединения-поиска
        public class Subset
        {
            public int parent, rank;
        }

        // Поиск набора элементов i (использует метод сжатия пути)
        public int Find(Subset[] subsets, int i)
        {
            // Найти корень и сделать корень родителем i (сжатие пути)
            if (subsets[i].parent != i)
                subsets[i].parent
                    = Find(subsets, subsets[i].parent);

            return subsets[i].parent;
        }

        // Объединение двух наборов x и y (использует объединение по рангу)
        public void Union(Subset[] subsets, int x, int y)
        {
            int xroot = Find(subsets, x);
            int yroot = Find(subsets, y);

            // Прикрепляем дерево меньшего ранга под корнем дерева высокого ранга (объединенияем по рангу)
            if (subsets[xroot].rank < subsets[yroot].rank)
                subsets[xroot].parent = yroot;
            else if (subsets[xroot].rank > subsets[yroot].rank)
                subsets[yroot].parent = xroot;

            // Если ранги одинаковы, то делаем его корневым и увеличиваем его ранг на единицу
            else
            {
                subsets[yroot].parent = xroot;
                subsets[xroot].rank++;
            }
        }

        public void ReadFile(string fileName)
        {
            using var reader = new StreamReader(fileName);
            string line;
            reader.ReadLine();
            //V = Convert.ToInt32(Regex.Match(line, @"\d+").Value);
            // E = (V * (V - 1)) / 2;
            int i = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] tokens = line.Split();
                verticleList[i] = new Verticle
                {
                    x_coord = int.Parse(tokens[0]),
                    y_coord = int.Parse(tokens[1])
                };
                i++;
            }
        }

        public static int CalcVertDistance(Verticle vert1, Verticle vert2)
        {
            return Math.Abs(vert1.x_coord - vert2.x_coord) + Math.Abs(vert1.y_coord - vert2.y_coord);
        }

        //--------------------------------------------Для нахождения связной компоненты длины k--------------------------------------------
        public static List<Edge> GetConnectedComponent(List<Edge> edges, int k)
        {
            var graph = new Dictionary<int, List<int>>();
            foreach (var edge in edges)
            {
                if (!graph.ContainsKey(edge.src))
                    graph[edge.src] = new List<int>();
                if (!graph.ContainsKey(edge.dest))
                    graph[edge.dest] = new List<int>();
                graph[edge.src].Add(edge.dest);
                graph[edge.dest].Add(edge.src);
            }

            var visited = new HashSet<int>();
            var result = new List<Edge>();
            foreach (var node in graph.Keys)
            {
                if (visited.Contains(node)) continue;
                var component = new HashSet<int>();
                Dfs(node, graph, visited, component);
                if (component.Count == k)
                {
                    result.AddRange(edges.Where(e => component.Contains(e.src) && component.Contains(e.dest)));
                    break;
                }
            }
            return result;
        }

        public static void Dfs(int node, Dictionary<int, List<int>> graph, HashSet<int> visited, HashSet<int> component)
        {
            visited.Add(node);
            component.Add(node);
            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                    Dfs(neighbor, graph, visited, component);
            }
        }

        // Главный метод
        public void KruskalMST(int k)
        {
            Edge[] result = new Edge[E];
            int resultIndex = 0;
            int i;

            for (i = 0; i < E; ++i)
                result[i] = new Edge();

            Array.Sort(edgeList);// Сортируем по возрастанию ребра

            // Создание подмножеств ребер
            Subset[] subsets = new Subset[V];
            for (int v = 0; v < V; ++v)
            {
                subsets[v] = new Subset
                {
                    parent = v,
                    rank = 0
                };
            }

            i = 0; // Для прохода по основному массиву с ребрами
            Edge[] resultFinish = new Edge[E]; // Искомое дерево с k вершинами и k-1 ребром
            while (resultIndex < V - 1)
            {
                // Берем наименьшее ребро + УВЕЛИЧИВАЕМ индекс для следующей итерации
                Edge next_edge = edgeList[i++];

                int x = Find(subsets, next_edge.src);
                int y = Find(subsets, next_edge.dest);

                // Если добавление этого ребра не создает цикл, то добавляем его в результат и увеличьте индекс результата для следующего ребра.
                if (x != y)
                {
                    result[resultIndex] = next_edge;

                    // После того, как добавили ребро в результирующий массив преобразуем его в List<Edge> и ищем связную компоненту длины k
                    List<Edge> separatedGraphEdgeList = new List<Edge>();
                    for (int j = 0; j < resultIndex + 1; j++)
                        separatedGraphEdgeList.Add(result[j]);

                    List<Edge> graphComponent = GetConnectedComponent(separatedGraphEdgeList, k);

                    if (!(graphComponent.Count == 0)) // Если нашлась связная компонента длины k, то преобразуем массив обратно в Edge[] и выходим из цикла, считая, что нашли ответ
                    {
                        for (int j = 0; j < graphComponent.Count; j++)
                            resultFinish[j] = graphComponent[j];
                        break;
                    }
                    resultIndex++;
                    Union(subsets, x, y);
                }
            }
            //---------------------------------------------------Результат---------------------------------------------------
            Console.WriteLine("Результат выполнения программы:");

            // Считаем число листьев в дереве
            Dictionary<int, int> uniqueVerticles = new Dictionary<int, int>();
            for (i = 0; i < k-1; ++i)
            {
                if (!uniqueVerticles.ContainsKey(resultFinish[i].src))
                    uniqueVerticles.Add(resultFinish[i].src, 0);
                if (!uniqueVerticles.ContainsKey(resultFinish[i].dest))
                    uniqueVerticles.Add(resultFinish[i].dest, 0);
            }
            for (i = 0; i < k-1; ++i)
            {
                uniqueVerticles[resultFinish[i].src]++;
                uniqueVerticles[resultFinish[i].dest]++;
            }
            int leafs = 0;
            foreach (var vertyy in uniqueVerticles)
            {
                if (vertyy.Value == 1)
                    leafs++;
            }

            int treeWeight = 0;
            for (i = 0; i < k-1; ++i) // Считаем вес дерева и выводим номера вершин построчно
            {
                Console.WriteLine("e" + " " + ++resultFinish[i].src + " " + ++resultFinish[i].dest);
                treeWeight += resultFinish[i].weight;
            }

            Console.WriteLine("c " + "Вес дерева = " + treeWeight + ", число листьев = " + leafs);
            Console.ReadLine();
        }
    }

    class Program
    {
        public static void Main(String[] args)
        {
            //int V = 64;   int E = 2016;
            //int V = 128;  int E = 8128;
            //int V = 512;  int E = 130816; // НА 512 ВЕРШИНАХ НЕ РАБОТАЕТ ПОЧЕМУ_ТО :P
            //int V = 2048; int E = 2096128;
            int V = 4096; int E = 8386560;
            Graph graph = new Graph(V, E);
            try
            { 
                graph.ReadFile(@"data\\Taxicab_4096.txt"); 
                int v2Index = 0;
                int edgeIndex = 0;
                for (int v1Index = 0; v1Index < V - 1; v1Index++)
                {
                    v2Index++;
                    Verticle vert1 = new Verticle
                    {
                        x_coord = graph.verticleList[v1Index].x_coord,
                        y_coord = graph.verticleList[v1Index].y_coord
                    };
                    for (int i = v2Index; i < V; i++)
                    {
                        Verticle vert2 = new Verticle
                        {
                            x_coord = graph.verticleList[i].x_coord,
                            y_coord = graph.verticleList[i].y_coord
                        };

                        int distance = CalcVertDistance(vert1, vert2);

                        graph.edgeList[edgeIndex] = new Edge
                        {
                            src = v1Index,
                            dest = i,
                            weight = distance
                        };
                        edgeIndex++;
                    }
                }
                int k = V / 8;
                graph.KruskalMST(k);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}