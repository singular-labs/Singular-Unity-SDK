//
//  SingularStateWrapper.h
//  Singular
//
//  Copyright © Singular Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Singular/SingularLinkParams.h>

NS_ASSUME_NONNULL_BEGIN

@interface SingularStateWrapper : NSObject

+(void)setLaunchOptions:(NSDictionary*) options;
+(NSDictionary*)getLaunchOptions;
+(void)clearLaunchOptions;
+(NSString*)getApiKey;
+(NSString*)getApiSecret;
+(void (^)(SingularLinkParams*))getSingularLinkHandler;
+(int)getShortlinkResolveTimeout;
+(BOOL)enableSingularLinks:(NSString*)key withSecret:(NSString*)secret andHandler:(void (^)(SingularLinkParams*))handler withTimeout:(int)timeoutSec;
+(BOOL)isSingularLinksEnabled;

@end

NS_ASSUME_NONNULL_END
