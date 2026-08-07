//
//  SingularSceneDelegate.m
//  Singular
//
//  Copyright © Singular Inc. All rights reserved.


#import <UIKit/UIKit.h>
#import <objc/runtime.h>
#import <Singular/Singular.h>
#import "SingularStateWrapper.h"

static NSString* const kUserActivityKey = @"UIApplicationLaunchOptionsUserActivityKey";

@interface SingularSceneDelegate : NSObject
@end

@implementation SingularSceneDelegate

static IMP originalSceneContinueUserActivity;
static IMP originalSceneWillConnect;

+ (BOOL)isBrowsingActivity:(NSUserActivity *)activity {
    return activity != nil &&
           [activity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb] &&
           activity.webpageURL != nil;
}

+ (void)handleUserActivity:(NSUserActivity *)userActivity {
    if (![SingularSceneDelegate isBrowsingActivity:userActivity] ||
        ![SingularStateWrapper isSingularLinksEnabled]) {
        return;
    }

    NSString* apiKey = [SingularStateWrapper getApiKey];
    NSString* apiSecret = [SingularStateWrapper getApiSecret];
    void (^singularLinkHandler)(SingularLinkParams*) = [SingularStateWrapper getSingularLinkHandler];
    int shortlinkResolveTimeout = [SingularStateWrapper getShortlinkResolveTimeout];

    if (shortlinkResolveTimeout <= 0) {
        [Singular startSession:apiKey
                       withKey:apiSecret
               andUserActivity:userActivity
       withSingularLinkHandler:singularLinkHandler];
    } else {
        [Singular startSession:apiKey
                       withKey:apiSecret
               andUserActivity:userActivity
       withSingularLinkHandler:singularLinkHandler
    andShortLinkResolveTimeout:shortlinkResolveTimeout];
    }
}

+ (NSUserActivity *)firstBrowsingActivity:(NSSet<NSUserActivity *> *)activities {
    for (NSUserActivity* activity in activities) {
        if ([SingularSceneDelegate isBrowsingActivity:activity]) {
            return activity;
        }
    }
    return nil;
}

static void sl_sceneContinueUserActivity(id self, SEL _cmd, UIScene* scene, NSUserActivity* userActivity) {
    [SingularSceneDelegate handleUserActivity:userActivity];

    if (originalSceneContinueUserActivity) {
        ((void(*)(id, SEL, UIScene*, NSUserActivity*))originalSceneContinueUserActivity)(self, _cmd, scene, userActivity);
    }
}

static void sl_sceneWillConnect(id self, SEL _cmd, UIScene* scene, UISceneSession* session, UISceneConnectionOptions* connectionOptions) {
    NSUserActivity* userActivity = [SingularSceneDelegate firstBrowsingActivity:connectionOptions.userActivities];
    if (userActivity != nil) {
        [SingularStateWrapper setLaunchOptions:@{
            UIApplicationLaunchOptionsUserActivityDictionaryKey: @{
                kUserActivityKey: userActivity
            }
        }];
    }

    if (originalSceneWillConnect) {
        ((void(*)(id, SEL, UIScene*, UISceneSession*, UISceneConnectionOptions*))originalSceneWillConnect)(self, _cmd, scene, session, connectionOptions);
    }
}

// Replace method on klass and return the previous IMP so callers can chain.
static IMP sl_replaceMethod(Class klass, SEL sel, IMP newImp, const char* typeEncoding) {
    Method method = class_getInstanceMethod(klass, sel);
    Class superKlass = class_getSuperclass(klass);
    Method superMethod = superKlass ? class_getInstanceMethod(superKlass, sel) : NULL;

    if (method && method != superMethod) {
        return method_setImplementation(method, newImp);
    }

    class_addMethod(klass, sel, newImp, method ? method_getTypeEncoding(method) : typeEncoding);
    return method ? method_getImplementation(method) : NULL;
}

+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        Class unityScene = NSClassFromString(@"UnityScene");
        if (unityScene == nil) {
            return;
        }

        originalSceneContinueUserActivity = sl_replaceMethod(unityScene,
            @selector(scene:continueUserActivity:),
            (IMP)sl_sceneContinueUserActivity,
            "v@:@@");

        originalSceneWillConnect = sl_replaceMethod(unityScene,
            @selector(scene:willConnectToSession:options:),
            (IMP)sl_sceneWillConnect,
            "v@:@@@");
    });
}

@end
