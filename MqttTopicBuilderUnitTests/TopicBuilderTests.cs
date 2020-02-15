﻿/**
 * Author
 *      Pierre Bouillon - https://github.com/pBouillon
 *
 * Repository
 *      MqttTopicBuilder - https://github.com/pBouillon/MqttTopicBuilder
 *
 * License
 *      MIT - https://github.com/pBouillon/MqttTopicBuilder/blob/master/LICENSE
 */

namespace MqttTopicBuilderUnitTests
{
    using AutoFixture;
    using FluentAssertions;
    using MqttTopicBuilder;
    using MqttTopicBuilder.Exceptions;
    using MqttTopicBuilder.MqttUtils;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    public class TopicBuilderTests
    {
        /// <summary>
        /// AutoFixture's object to generate fixtures
        /// </summary>
        /// <see cref="Fixture"/>
        private readonly Fixture _fixture = new Fixture();

        /// <summary>
        /// Check if an exception is correctly raised on the addition of a blank topic
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_BlankTopic()
        {
            // Arrange
            const string topic = " ";
            var builder = new TopicBuilder();

            // Act
            Action addBlankTopic = ()
                => builder.AddTopic(topic);

            // Assert
            addBlankTopic.Should()
                .Throw<EmptyTopicException>(
                    "because a topic made of spaces added should result in an exception");
        }

        /// <summary>
        /// Check if an exception is correctly raised on the addition of an empty topic
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_EmptyTopic()
        {
            // Arrange
            var topic = string.Empty;
            var builder = new TopicBuilder();

            // Act
            Action addEmptyTopic = ()
                => builder.AddTopic(topic);

            // Assert
            addEmptyTopic.Should()
                .Throw<EmptyTopicException>(
                    "because an empty topic added should result in an exception");
        }

        /// <summary>
        /// Check if an exception is correctly raised on the addition of a topic after the builder locked it
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_IllegalTopicAppending()
        {
            // Arrange
            var builder = new TopicBuilder();

            // Act
            Action illegalTopicAppending = ()
                => builder.AddTopic(_fixture.Create<string>())
                    // appending should be forbidden after here
                    .AddWildcardMultiLevel() 
                    .AddTopic(_fixture.Create<string>());

            // Assert
            illegalTopicAppending.Should()
                .Throw<IllegalTopicConstructionException>(
                    "because a topic added after a multi level wildcard should result in an exception");
        }

        /// <summary>
        /// Check the builder's ability to add a valid topic
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_ValidSingleLevelTopic()
        {
            // Arrange
            var topic = _fixture.Create<string>();
            topic = Topic.Normalize(topic);

            var builder = new TopicBuilder();

            // Act
            builder.AddTopic(topic);

            // Assert
            builder.Level.Should()
                .Be(1, "because exactly one element should have been added");
        }

        /// <summary>
        /// Check if an exception is correctly raised on the addition of a topic containing a separator
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_WithSeparator()
        {
            // Arrange
            var topic = new StringBuilder()
                .Append(_fixture.Create<string>())
                .Append(Topics.Separator)
                .Append(_fixture.Create<string>())
                .ToString();
            var builder = new TopicBuilder();
            
            // Act
            Action addTopicWithSeparator = ()
                => builder.AddTopic(topic);

            // Assert
            addTopicWithSeparator.Should()
                .Throw<InvalidTopicException>(
                    "because a topic containing a topic separator added should result in an exception.");
        }

        /// <summary>
        /// Check if an exception is correctly raised on the addition of a topic containing a wildcard (multi level)
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_WithWildcardMultiLevel()
        {
            // Arrange
            var topic = new StringBuilder()
                .Append(_fixture.Create<string>())
                .Append(Wildcards.MultiLevel)
                .Append(_fixture.Create<string>())
                .ToString();
            var builder = new TopicBuilder();

            // Act
            Action addTopicWithSeparator = ()
                => builder.AddTopic(topic);

            // Assert
            addTopicWithSeparator.Should()
                .Throw<InvalidTopicException>(
                    "because a topic containing a wildcard (multi level) should result in an exception.");
        }

        /// <summary>
        /// Check if an exception is correctly raised on the addition of a topic containing a wildcard (single level)
        /// </summary>
        [Fact]
        public void TopicBuilder_AddTopic_WithWildcardSingleLevel()
        {
            // Arrange
            var topic = new StringBuilder()
                .Append(_fixture.Create<string>())
                .Append(Wildcards.SingleLevel)
                .Append(_fixture.Create<string>())
                .ToString();
            var builder = new TopicBuilder();

            // Act
            Action addTopicWithSeparator = ()
                => builder.AddTopic(topic);

            // Assert
            addTopicWithSeparator.Should()
                .Throw<InvalidTopicException>(
                    "because a topic containing a wildcard (single level) should result in an exception.");
        }

        /// <summary>
        /// Check if single level wildcard appending is correctly appended
        /// </summary>
        [Fact]
        public void TopicBuilder_AddWildcardSingleLevel_AddToEmptyBuilder()
        {
            // Arrange
            var builder = new TopicBuilder();

            // Act
            builder.AddWildcardSingleLevel();

            // Assert
            builder.Level.Should()
                .Be(1,
                    "because we only append data once");

            builder.Build().Path.Should()
                .Be(Wildcards.SingleLevel.ToString(),
                    "because the wildcard must have been used in this macro");
        }

        /// <summary>
        /// Check if the addition of a single level wildcard correctly raise an exception after a multi level wildcard
        /// </summary>
        [Fact]
        public void TopicBuilder_AddWildcardSingleLevel_BlockAdditionAfterMultiLevelWildcard()
        {
            // Arrange
            var builder = new TopicBuilder();
            builder.AddWildcardMultiLevel();

            // Act
            Action appendingAfterMultiLevelWildcard = ()
                => builder.AddWildcardSingleLevel();

            // Assert
            appendingAfterMultiLevelWildcard.Should()
                .Throw<IllegalTopicConstructionException>(
                    "because no topics should ever be append after a multi level wildcard");
        }

        /// <summary>
        /// Check if the smallest topic is built when the builder does not contains any data
        /// </summary>
        [Fact]
        public void TopicBuilder_Build_BuildFromEmptyTopic()
        {
            // Arrange
            var builder = new TopicBuilder();

            // Act
            var result = builder.Build();

            // Assert
            result.Path.Should()
                .Be(Topics.Separator.ToString(),
                    "because a builder with no topic staged should build the smallest one, even if '/' as a topic is deprecated");
        }

        /// <summary>
        /// Check the builder's ability to generate a valid topic
        /// </summary>
        [Fact]
        public void TopicBuilder_Build_ValidMultipleLevelsTopic()
        {
            // Arrange
            var builder = new TopicBuilder();
            
            var topics = new Queue<string>();

            for (var i = 0; i < _fixture.Create<int>(); ++i)
            {
                topics.Enqueue(_fixture.Create<string>());
            }

            var expectedTopic = string.Join(Topics.Separator, topics);

            // Act
            foreach (var topic in topics)
            {
                builder.AddTopic(topic);
            }

            // Assert
            builder.Level.Should()
                .Be(builder.Build().Level, 
                    "because there should be as much elements as elements added");

            builder.Build().Path.Should()
                .Be(expectedTopic,
                    "because appending simple topics should be them of joint by the separator");
        }

        /// <summary>
        /// Check the initializations when the default constructor is called
        /// </summary>
        [Fact]
        public void TopicBuilder_Constructor_Default()
        {
            // Arrange + Act
            var builder = new TopicBuilder();
            
            // Assert
            builder.IsEmpty.Should()
                .BeTrue("because invoke the default constructor with no parameters must result in an empty queue");

            builder.Level.Should()
                .Be(0, "because the empty builder shouldn't be of any level");
        }

        /// <summary>
        /// Check the initializations when the constructor is called with a specified stack size
        /// </summary>
        [Fact]
        public void TopicBuilder_Constructor_SpecifiedQueueSize()
        {
            // Arrange
            var queueSize = _fixture.Create<int>();

            // Act
            var builder = new TopicBuilder(queueSize);

            // Assert
            builder.IsEmpty.Should()
                .BeTrue("because invoke the default constructor with the queue's size must result in an empty queue");

            builder.Level.Should()
                .Be(0, "because the empty builder shouldn't be of any level");
        }
    }
}
