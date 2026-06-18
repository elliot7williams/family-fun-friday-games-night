//
//  Home.swift
//  Family Fun Friday Games Night
//
//  Created by Elliot Williams on 2017-12-19.
//  Copyright © 2017 Elliot Williams. All rights reserved.
//
import Foundation
import UIKit
import WebKit

extension UIDevice {
    /// Checks if we run in Mac Catalyst Optimized For Mac Idiom
    var isCatalystMacIdiom: Bool {
        if #available(iOS 14, *) {
            return UIDevice.current.userInterfaceIdiom == .mac
        } else {
            return false
        }
    }
}

class Home: UIViewController {
    @IBOutlet weak fileprivate var webView: WKWebView!
    var backgroundTask: UIBackgroundTaskIdentifier = UIBackgroundTaskIdentifier.invalid
    let rC = UIRefreshControl()
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        webView.load(URLRequest(url: URL(fileURLWithPath: Bundle.main.path(forResource: "index", ofType: "html")!)))
        
        if #available(iOS 13.0, *) {
            //if traitCollection.userInterfaceStyle == .dark {
                overrideUserInterfaceStyle = .dark
            //view.backgroundColor = UIColor.black
            //}
        } else if #available(iOS 11.3, *) {
            //webView.accessibilityIgnoresInvertColors = true
        } else if #available(iOS 11.0, *) {
            webView.accessibilityIgnoresInvertColors = true
        } else {
            // Fallback on earlier versions
        }
        rC.addTarget(self, action: #selector(rWV(_:)), for: UIControl.Event.valueChanged)
        webView.scrollView.addSubview(rC)
        webView.scrollView.bounces = true
        //view = webView
        //let request = URLRequest(url: url!)
        //webView.load(request)
        //loadW(withURLString: "https://gamesnight.weebly.com/")
    }
    
    func registerBackgroundTask() {
        backgroundTask = UIApplication.shared.beginBackgroundTask { [weak self] in
            self?.endBackgroundTask()
        }
        assert(backgroundTask != UIBackgroundTaskIdentifier.invalid)
    }
    
    func endBackgroundTask() {
        //print("Background task ended.")
        UIApplication.shared.endBackgroundTask(convertToUIBackgroundTaskIdentifier(backgroundTask.rawValue))
        backgroundTask = UIBackgroundTaskIdentifier.invalid
    }
    //func loadW(withURLString urlString: String) {
        //let webURL = URL(string: urlString)
        //let webURLRequest = URLRequest(url: webURL!)
        //webView.load(webURLRequest)
    //}
    //override var prefersStatusBarHidden: Bool {
       // return true
    //}
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    @objc func rWV(_ sender: UIRefreshControl) {
        webView?.reload()
        sender.endRefreshing()
    }
}
// Helper function inserted by Swift 4.2 migrator.
fileprivate func convertToUIBackgroundTaskIdentifier(_ input: Int) -> UIBackgroundTaskIdentifier {
	return UIBackgroundTaskIdentifier(rawValue: input)
}
