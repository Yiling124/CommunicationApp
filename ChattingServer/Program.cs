﻿/////////////////////////////////////////////////////////////////////////////                                     //
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                              //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package starts the server of the application 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChattingServer
{
    class Program 
    {
        public static ChattingService _server;
        static void Main(string[] args)
        {
            try
            {
                _server = new ChattingService();
                using (ServiceHost host = new ServiceHost(_server))
                {
                    host.Open();
                    Console.WriteLine("Server is running....");
                    Console.ReadLine();
                }
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
            }
        }
    }
}
