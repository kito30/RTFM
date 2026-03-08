
import { SignalRProvider } from "./context/signalRContext";
import "./globals.css";



export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <SignalRProvider>
          {children}
        </SignalRProvider>
      </body>
    </html>
  );
}
