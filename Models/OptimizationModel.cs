using System;
using System.Data;
using Newtonsoft.Json;


namespace WebApiCoreCode
{
    public class OptimizationModel
    {
        GeneralizedAssignmentProblem problem;

        public GeneralizedAssignmentProblem readJson(string path) 
        {
            problem = JsonConvert.DeserializeObject<GeneralizedAssignmentProblem>(System.IO.File.ReadAllText(path));
            return problem;
        }

        public string writeSol(int[] sol) {
            return String.Join(',', sol)+ " " + this.checkSol(sol);
        }

        public int checkSol(int[] sol)
        {  
            int z = 0, j;
            int[] capused = new int[problem.numserv];
            for(int i=0;i<problem.numserv;i++) capused[i] = 0;
            // controllo assegnamenti
            for (j = 0; j < problem.numcli; j++)
                if (sol[j] < 0 || sol[j] >= problem.numserv)
                {  
                    //return int.MaxValue;
                    throw new Exception("Server not exists");
                } 
                else
                    z += problem.cost[sol[j],j];
            // controllo capacità
            for (j = 0; j < problem.numcli; j++) 
            {  
                capused[sol[j]] += problem.req[sol[j],j];
                if (capused[sol[j]] > problem.cap[sol[j]]) 
                {  
                    //return int.MaxValue;
                    throw new Exception("Server capacity exceeded");
                }
            }
            return z;
        }
    
        public int[] findSol()
        {
            int[] sol = new int[problem.numcli];

            int[] ind = new int[problem.numserv];
            float[] keys = new float[problem.numserv];

            int[] capLeft = (int[])problem.cap.Clone();

            //for(int i=0;i<problem.numserv;i++) dist[i] = new Array(2);
            for(int j=0;j<problem.numcli;j++)
            {  
                sol[j] = -1;

                for(int i=0;i<problem.numserv;i++)
                {  
                    keys[i] = problem.req[i,j];//problem.cost[i,j]; // problem.req[i,j];
                    ind[i] = i;
                }
                Array.Sort(keys, ind);   
                for(int ii = 0;ii<problem.numserv;ii++)
                {  
                    int i = ind[ii];
                    if(capLeft[i] >= problem.req[i,j])
                    { 
                        capLeft[i] -= problem.req[i,j];
                        sol[j] = i;
                        break;
                    }
                }
                
            }
            return sol;
        }
        public int[] Gap10(int[] sol)
        {

             for(int j=0;j<problem.numcli;j++)
            {   
                for(int i = 0;i<problem.numserv;i++)
                {  
                    if(sol[j]==i) continue;
                    int[] tmpsol = (int[])sol.Clone(); 
                    tmpsol[j]= i;
                    try
                    {
                        int cost = checkSol(tmpsol);
                        int oldcost = checkSol(sol);
                        if(cost<oldcost){
                            return Gap10(tmpsol);
                        }
                    }
                    catch
                    {  
                    }
                }
            }           
            return sol;
        }
        public int[] SimulatedAnnealing(int[] sol)
        {
            int[] optsol = null;
            int optcost = Int32.MaxValue;

            Random r = new Random();
            bool firstStep = true;
            double p = 0.6;
            double T = 0;
            
            int totalSteps = 0, step = 0;

            while(step <= 5 || p > 0.0001)
            {
                int[] tmpsol = (int[])sol.Clone();
                tmpsol[r.Next(problem.numcli)] = r.Next(problem.numserv);
                try
                {
                    int cost = checkSol(tmpsol);
                    int oldcost = checkSol(sol);

                    if(cost<=oldcost){
                        sol = tmpsol;
                        if (cost < optcost) {
                            optsol = sol;
                            optcost = cost;
                            step = 0;
                        }
                    } else {
                        if (firstStep) {
                            T = -(cost-oldcost)/Math.Log(p);
                            firstStep = false;
                        } else
                            p = Math.Exp(-(cost-oldcost)/(double)T);

                        if(r.Next()/(float)Int32.MaxValue<p){
                            sol = tmpsol;
                        }
                    }
                }
                catch
                {
                }
                step++;
                totalSteps++;
                if(totalSteps % (problem.numcli*(problem.numcli-1)) == 0) T *=0.95;
            }
            return optsol;
        }
    }
}