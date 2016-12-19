﻿using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System.Collections.Generic;

namespace RedditSharp
{
    using System;
    using System.Threading.Tasks;

    public class Wiki
    {
        private Reddit Reddit { get; set; }
        private Subreddit Subreddit { get; set; }
        private IWebAgent WebAgent { get; set; }
        #region constants
        private const string GetWikiPageUrl = "/r/{0}/wiki/{1}.json?v={2}";
        private const string GetWikiPagesUrl = "/r/{0}/wiki/pages.json";
        private const string WikiPageEditUrl = "/r/{0}/api/wiki/edit";
        private const string HideWikiPageUrl = "/r/{0}/api/wiki/hide";
        private const string RevertWikiPageUrl = "/r/{0}/api/wiki/revert";
        private const string WikiPageAllowEditorAddUrl = "/r/{0}/api/wiki/alloweditor/add";
        private const string WikiPageAllowEditorDelUrl = "/r/{0}/api/wiki/alloweditor/del";
        private const string WikiPageSettingsUrl = "/r/{0}/wiki/settings/{1}.json";
        private const string WikiRevisionsUrl = "/r/{0}/wiki/revisions.json";
        private const string WikiPageRevisionsUrl = "/r/{0}/wiki/revisions/{1}.json";
        private const string WikiPageDiscussionsUrl = "/r/{0}/wiki/discussions/{1}.json";
        #endregion
        public async Task<IEnumerable<string>> GetPageNamesAsync()
        {
            var request = WebAgent.CreateGet(string.Format(GetWikiPagesUrl, Subreddit.Name));
            var response = await WebAgent.GetResponseAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return JObject.Parse(json)["data"].Values<string>();
        }

        public Listing<WikiPageRevision> Revisions
        {
            get
            {
                return new Listing<WikiPageRevision>(Reddit, string.Format(WikiRevisionsUrl, Subreddit.Name), WebAgent);
            }
        }

        protected internal Wiki(Reddit reddit, Subreddit subreddit, IWebAgent webAgent)
        {
            Reddit = reddit;
            Subreddit = subreddit;
            WebAgent = webAgent;
        }

        public async Task<WikiPage> GetPageAsync(string page, string version = null)
        {
            var request = WebAgent.CreateGet(string.Format(GetWikiPageUrl, Subreddit.Name, page, version));
            var response = await WebAgent.GetResponseAsync(request);
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = new WikiPage(Reddit, json["data"], WebAgent);
            return result;
        }

        #region Settings
        public async Task<WikiPageSettings> GetPageSettingsAsync(string name)
        {
            var request = WebAgent.CreateGet(string.Format(WikiPageSettingsUrl, Subreddit.Name, name));
            var response = await WebAgent.GetResponseAsync(request);
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = new WikiPageSettings(Reddit, json["data"], WebAgent);
            return result;
        }

        public Task SetPageSettingsAsync(string name, WikiPageSettings settings)
        {
            var request = WebAgent.CreatePost(string.Format(WikiPageSettingsUrl, Subreddit.Name, name));
            WebAgent.WritePostBody(request, new
            {
                page = name,
                permlevel = settings.PermLevel,
                listed = settings.Listed,
                uh = Reddit.User.Modhash
            });
            return WebAgent.GetResponseAsync(request);
        }
        #endregion

        #region Revisions

        public Listing<WikiPageRevision> GetPageRevisions(string page)
        {
            return new Listing<WikiPageRevision>(Reddit, string.Format(WikiPageRevisionsUrl, Subreddit.Name, page), WebAgent);
        }
        #endregion

        #region Discussions
        public Listing<Post> GetPageDiscussions(string page)
        {
            return new Listing<Post>(Reddit, string.Format(WikiPageDiscussionsUrl, Subreddit.Name, page), WebAgent);
        }
        #endregion

        public Task EditPageAsync(string page, string content, string previous = null, string reason = null)
        {
            var request = WebAgent.CreatePost(string.Format(WikiPageEditUrl, Subreddit.Name));
            dynamic param = new
            {
                content = content,
                page = page,
                uh = Reddit.User.Modhash
            };
            List<string> addParams = new List<string>();
            if (previous != null)
            {
                addParams.Add("previous");
                addParams.Add(previous);
            }
            if (reason != null)
            {
                addParams.Add("reason");
                addParams.Add(reason);
            }
            WebAgent.WritePostBody(request, param, addParams.ToArray());
            return WebAgent.GetResponseAsync(request);
        }
        public Task HidePageAsync(string page, string revision)
        {
            var request = WebAgent.CreatePost(string.Format(HideWikiPageUrl, Subreddit.Name));
            WebAgent.WritePostBody(request, new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
            return WebAgent.GetResponseAsync(request);
        }
        public Task RevertPageAsync(string page, string revision)
        {
            var request = WebAgent.CreatePost(string.Format(RevertWikiPageUrl, Subreddit.Name));
            WebAgent.WritePostBody(request, new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
            return WebAgent.GetResponseAsync(request);
        }
        public Task SetPageEditorAsync(string page, string username, bool allow)
        {
            var request = WebAgent.CreatePost(string.Format(allow ? WikiPageAllowEditorAddUrl : WikiPageAllowEditorDelUrl, Subreddit.Name));
            WebAgent.WritePostBody(request, new
            {
                page = page,
                username = username,
                uh = Reddit.User.Modhash
            });
            return WebAgent.GetResponseAsync(request);
        }

    }
}