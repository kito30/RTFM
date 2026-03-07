import * as signalR from "@microsoft/signalr";
import { useState } from "react";

export default function Dasboard() {
    const connection = new signalR.HubConnectionBuilder()
    const [cpuUsage, setCpuUsage] = useState(0);
    return (
      <div>
        <div className="text-2xl font-bold mb-4">
          Computer Resousces
        </div>  
        <div>
          <div> CPU: </div>
          <div> </div>
        </div>
 
      </div>
   );
}