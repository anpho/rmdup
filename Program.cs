// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Security.Cryptography;

namespace anpho
{

    class Node

    {
        /// <summary>
        /// Get MD5 hash of a file
        /// </summary>
        /// <param name="filepath">File Path</param>
        /// <returns>null if error happened.</returns>
        static string? GetMD5Hash(string filepath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                try
                {
                    using (var stream = System.IO.File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                    return null;
                }
            }
        }

        /// <summary>
        /// Get file size
        /// </summary>
        /// <param name="filepath">File Path</param>
        /// <returns></returns>
        static long GetFileSize(string filepath)
        {
            var fileInfo = new FileInfo(filepath);
            return fileInfo.Length;
        }
        public string filePath;
        public long fileSize;
        public string? fileHash;
        public Node? sameWith;

        /// <summary>
        /// Node constructor
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <exception cref="Exception">If file doesn't exists.</exception>
        public Node(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == false)
            {
                throw new Exception("File not exist");
            }

            this.filePath = filePath;
            this.fileSize = GetFileSize(filePath);
            this.sameWith = null;
        }
        /// <summary>
        /// Get file hash
        /// </summary>
        /// <returns>null if error happens.</returns>
        public string? getMyHash()
        {
            if (fileHash != null) return fileHash;
            else
            {
                fileHash = GetMD5Hash(filePath);
                return fileHash;
            }
        }
    }

    public class Program
    {
        static bool justDelete = false;

        static void Main(string[] args)
        {
            var current_Directory = Directory.GetCurrentDirectory();

            if (args.Length > 0)
            {
                if (Directory.Exists(args[0]))
                {
                    current_Directory = args[0];
                }
            }

            Console.WriteLine($"Finding duplicate files in [{current_Directory}]");

            var huge_hash_table = new Dictionary<long, List<Node>>();
            var to_delete_list = new List<Node>();
            var file_count = 0;


            var files = Directory.GetFiles(current_Directory, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                file_count++;
                if (file_count % 1000 == 0)
                {
                    Console.Write($"\rProcessed {file_count} files");
                }

                var node = new Node(file);
                /*
                * Check if fileSize exists in hash table
                * If yes, check md5
                * [filesize] = [node1, node2, node3,...] with same fileSize, but different MD5
                */
                if (huge_hash_table.ContainsKey(node.fileSize))
                {
                    // fileSize match, check md5
                    var obj = huge_hash_table[node.fileSize];
                    bool dup_flag = false;

                    foreach (var item in obj)
                    {
                        var nodeHash = node.getMyHash();
                        var itemHash = item.getMyHash();

                        if (nodeHash == null || itemHash == null)
                        {
                            continue;
                        }

                        if (node.getMyHash() == item.getMyHash())
                        {
                            //md5 match, mark as duplicate
                            node.sameWith = item;
                            dup_flag = true;
                            break;
                        }
                    }

                    if (dup_flag)
                    {
                        // found duplicate, add to to_delete_list
                        to_delete_list.Add(node);
                    }
                    else
                    {
                        // same filesize, but not same MD5, add to this huge_hash_table[filesize]
                        huge_hash_table[node.fileSize].Add(node);
                    }
                }
                else
                {
                    // new filesize, add to huge_hash_table
                    huge_hash_table.Add(node.fileSize, [node]);
                }
            }
            Console.WriteLine($"\rProcessed {file_count} files");
            Console.WriteLine($"Found {to_delete_list.Count} duplicate files");
            Console.WriteLine("----------------------------------");

            double total_size_in_MB = 0;

            foreach (var item in to_delete_list)
            {
                total_size_in_MB += item.fileSize / 1024.0 / 1024.0;

                Console.WriteLine($"[{item.sameWith?.filePath}] has duplicates to be removed at: {item.filePath}");
            }
            Console.WriteLine("----------------------------------");

            Console.WriteLine($"Total size: {total_size_in_MB:F2} MB");

            if (justDelete)
            {
                foreach (var item in to_delete_list)
                {
                    Console.WriteLine($"Deleting {item.filePath}");
                    File.Delete(item.filePath);
                }
                Console.WriteLine("Done.");
            }
            else
            {
                if (to_delete_list.Count == 0)
                {
                    Console.WriteLine("No duplicate files found.");
                }
                else
                {
                    Console.Write($"Found {to_delete_list.Count} duplicate files, delete?(Y/N)");

                    var input = Console.ReadLine();
                    if (input.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var item in to_delete_list)
                        {
                            Console.WriteLine($"Deleting {item.filePath}");
                            File.Delete(item.filePath);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Abort");
                    }
                }
            }
        }
    }
}