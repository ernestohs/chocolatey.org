﻿// Copyright 2011 - Present RealDimensions Software, LLC, the original 
// authors/contributors from ChocolateyGallery
// at https://github.com/chocolatey/chocolatey.org,
// and the authors/contributors of NuGetGallery 
// at https://github.com/NuGet/NuGetGallery
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data.Entity;
using System.Linq;
using OData.Linq;
using QueryInterceptor;

namespace NuGetGallery
{
    public static class PackageExtensions
    {
        private static readonly DateTime magicDateThatActuallyMeansUnpublishedBecauseOfLegacyDecisions = new DateTime(1900, 1, 1, 0, 0, 0);

        public static IQueryable<V1FeedPackage> ToV1FeedPackageQuery(this IQueryable<Package> packages, string siteRoot)
        {
            siteRoot = EnsureTrailingSlash(siteRoot);
            return packages.Include(p => p.PackageRegistration).WithoutNullPropagation().Select(
                p => new V1FeedPackage
                {
                    Id = p.PackageRegistration.Id,
                    Version = p.Version,
                    Authors = p.FlattenedAuthors,
                    Copyright = p.Copyright,
                    Created = p.Created,
                    Dependencies = p.FlattenedDependencies,
                    Description = p.Description,
                    DownloadCount = p.PackageRegistration.DownloadCount,
                    ExternalPackageUrl = p.ExternalPackageUrl,
                    GalleryDetailsUrl = siteRoot + "packages/" + p.PackageRegistration.Id + "/" + p.Version,
                    IconUrl = p.IconUrl,
                    IsLatestVersion = p.IsLatestStable,
                    Language = p.Language,
                    LastUpdated = p.LastUpdated,
                    LicenseUrl = p.LicenseUrl,
                    PackageHash = p.Hash,
                    PackageHashAlgorithm = p.HashAlgorithm,
                    PackageSize = p.PackageFileSize,
                    ProjectUrl = p.ProjectUrl,
                    Published = p.Listed ? p.Published : magicDateThatActuallyMeansUnpublishedBecauseOfLegacyDecisions,
                    ReleaseNotes = p.ReleaseNotes,
                    ReportAbuseUrl = siteRoot + "package/ReportAbuse/" + p.PackageRegistration.Id + "/" + p.Version,
                    RequireLicenseAcceptance = p.RequiresLicenseAcceptance,
                    Summary = p.Summary,
                    Tags = p.Tags == null ? null : " " + p.Tags.Trim() + " ", // In the current feed, tags are padded with a single leading and trailing space 
                    Title = p.Title ?? p.PackageRegistration.Id, // Need to do this since the older feed always showed a title.
                    VersionDownloadCount = p.DownloadCount,
                    Rating = 0
                });
        }

        public static IQueryable<V2FeedPackage> ToV2FeedPackageQuery(this IQueryable<Package> packages, string siteRoot)
        {
            siteRoot = EnsureTrailingSlash(siteRoot);
            //var rejectedStatus = PackageStatusType.Rejected.GetDescriptionOrValue();
            return packages
                //.Where(p => !p.StatusForDatabase.Equals(rejectedStatus,StringComparison.InvariantCultureIgnoreCase))
                .Include(p => p.PackageRegistration).WithoutNullPropagation().Select(
                    p => new V2FeedPackage
                    {
                        Id = p.PackageRegistration.Id,
                        Version = p.Version,
                        Authors = p.FlattenedAuthors,
                        Copyright = p.Copyright,
                        Created = p.Created,
                        Dependencies = p.FlattenedDependencies,
                        Description = p.Description,
                        DownloadCount = p.PackageRegistration.DownloadCount,
                        GalleryDetailsUrl = siteRoot + "packages/" + p.PackageRegistration.Id + "/" + p.Version,
                        IconUrl = p.IconUrl,
                        IsLatestVersion = p.IsLatestStable, // To maintain parity with v1 behavior of the feed, IsLatestVersion would only be used for stable versions.
                        IsAbsoluteLatestVersion = p.IsLatest,
                        IsPrerelease = p.IsPrerelease,
                        LastUpdated = p.LastUpdated,
                        LicenseUrl = p.LicenseUrl,
                        Language = p.Language,
                        PackageHash = p.Hash,
                        PackageHashAlgorithm = p.HashAlgorithm,
                        PackageSize = p.PackageFileSize,
                        ProjectUrl = p.ProjectUrl,
                        ProjectSourceUrl = p.ProjectSourceUrl,
                        PackageSourceUrl = p.PackageSourceUrl,
                        DocsUrl = p.DocsUrl,
                        MailingListUrl = p.MailingListUrl,
                        BugTrackerUrl = p.BugTrackerUrl,
                        ReleaseNotes = p.ReleaseNotes,
                        PackageStatus = p.StatusForDatabase,
                        PackageSubmittedStatus = p.SubmittedStatusForDatabase,
                        ReportAbuseUrl = siteRoot + "package/ReportAbuse/" + p.PackageRegistration.Id + "/" + p.Version,
                        RequireLicenseAcceptance = p.RequiresLicenseAcceptance,
                        Published = p.Listed ? p.Published : magicDateThatActuallyMeansUnpublishedBecauseOfLegacyDecisions,
                        Summary = p.Summary,
                        Tags = p.Tags,
                        Title = p.Title,
                        VersionDownloadCount = p.DownloadCount
                    });
        }

        internal static IQueryable<TVal> WithoutVersionSort<TVal>(this IQueryable<TVal> feedQuery)
        {
            return feedQuery.InterceptWith(new ODataRemoveVersionSorter());
        }

        private static string EnsureTrailingSlash(string siteRoot)
        {
            if (!siteRoot.EndsWith("/", StringComparison.Ordinal)) siteRoot = siteRoot + '/';
            return siteRoot;
        }
    }
}
