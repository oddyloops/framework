using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    /// <summary>
    /// Creates an abstraction for interacting with graph-oriented data sources
    /// </summary>
    public interface IGraphDataContext : IDisposable
    {
        /// <summary>
        /// Adds a new vertex to the graph
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <param name="vertex">Vertex object</param>
        /// <returns>A status indicating the success/failur of operation</returns>
        Task<IStatus<int>> AddVertexAsync<T>(T vertex);

        /// <summary>
        /// Removes a vertex from the graph along with all its edges
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <param name="vertex">Vertex object</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> RemoveVertexAsync<T>(T vertex);

        /// <summary>
        /// Removes a vertex from the graph along with all its edges
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <param name="vertex">Vertex object</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> RemoveVertexAsync<T,K>(K v);

        /// <summary>
        /// Replaces existing vertex with update version while maintaining its edges
        /// </summary>
        /// <typeparam name="T">Vertext type</typeparam>
        /// <param name="updatedVertex">Updated vertex</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> UpdateVertexAsync<T>(T updatedVertex);

        /// <summary>
        /// Gets vertex with matching key
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <param name="key">Vertex key</param>
        /// <returns>Vertex with matching key</returns>
        Task<T> GetVertexAsync<T, K>(K key);

        /// <summary>
        /// Connects source vertex to destination vertex with new edge
        /// </summary>
        /// <typeparam name="T">Vertices type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <param name="edge">Connecting edge</param>
        /// <param name="sourceVertex">Vertex establishing connection</param>
        /// <param name="destVertex">Vertex being connected to</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> AddEdgeAsyc<T, E>(E edge, T sourceVertex, T destVertex);

        /// <summary>
        /// Deletes edge from source vertex
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <param name="edge">Edge being removed</param>
        /// <param name="sourceVertex">Edge owner</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> RemoveEdgeAsync<T,E>(E edge, T sourceVertex);

        /// <summary>
        /// Deletes edge matching specified key from source vertex
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <typeparam name="K">Edge key type</typeparam>
        /// <param name="edgeKey">Edge record key</param>
        /// <param name="sourceVertex">Edge owner</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> RemoveEdgeAsync<T, E, K>(K edgeKey, T sourceVertex);

        /// <summary>
        /// Replaces existing edge with new one while maintaining the vertices at both ends
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <param name="updatedEdge">Updated edge</param>
        /// <param name="sourceVertex">Edge owner</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> UpdateEdgeAsync<T, E>(E updatedEdge, T sourceVertex);

        /// <summary>
        /// Retrieves edge with matching key and its destination vertex
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <typeparam name="K">Edge key typ</typeparam>
        /// <param name="edgeKey">Edge record key</param>
        /// <param name="sourceVertex">Edge owner</param>
        /// <returns>A tuple of edge and its destination vertex</returns>
        Task<Tuple<E, T>> GetEdgeAsync<T, E, K>(K edgeKey, T sourceVertex);

        /// <summary>
        /// Retrieves connected vertex matching specified vertex key along with the edge
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <typeparam name="K">Vertex key type</typeparam>
        /// <param name="destinationKey">Connected vertex key</param>
        /// <param name="sourceVertex">Connection owner</param>
        /// <returns>A tuple of edge and its destination vertex</returns>
        Task<Tuple<E, T>> GetConnectionAsync<T, E, K>(K destinationKey, T sourceVertex);

        /// <summary>
        /// Retrieves all edges belonging to vertex
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <param name="sourceVertex">Edges owner</param>
        /// <returns>A map of edges and their respective destination vertex</returns>
        Task<IDictionary<E, T>> GetAllEdgesAsync<T, E>(T sourceVertex);

        /// <summary>
        /// Retrieves a paged subset of edges belonging to vertex
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <typeparam name="E">Edge type</typeparam>
        /// <param name="sourceVertex">Edges owner</param>
        /// <param name="page">Page offset</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>A map of edges and their respective destination vertex</returns>
        Task<IDictionary<E, T>> GetEdgesRangeAsync<T, E>(T sourceVertex, int page, int pageSize);
    }
}
