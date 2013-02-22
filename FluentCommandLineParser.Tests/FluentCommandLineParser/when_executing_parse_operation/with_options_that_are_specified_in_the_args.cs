﻿using System.Collections.Generic;
using System.Linq;
using Fclp.Internals;
using Fclp.Tests.FluentCommandLineParser.TestContext;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Fclp.Tests.FluentCommandLineParser
{
    namespace when_executing_parse_operation
    {
        class with_options_that_are_specified_in_the_args : FluentCommandLineParserTestContext
        {
            static ICommandLineParserResult result;
            static string[] args = null;

            static Mock<ICommandLineOption> _blankOption = new Mock<ICommandLineOption>();
            static string _blankOptionName = "blankOption";
            static string _blankOptionValue = "blank Option Value";

            static Mock<ICommandLineOption> _optionThatHasCallback = new Mock<ICommandLineOption>();
            static string _optionThatHasCallbackName = "optionThatHasCallback";
            static string _optionThatHasCallbackValue = "Callback Value";

            static Mock<ICommandLineOption> _optionThatIsRequired = new Mock<ICommandLineOption>();
            static string _optionThatIsRequiredName = "optionThatIsRequired";
            static string _optionThatIsRequiredValue = "Is required value";

            Establish context = () =>
            {
                // create item that has a callback - the bind value should be executed
                _optionThatHasCallback.SetupGet(x => x.ShortName).Returns(_optionThatHasCallbackName);
                _optionThatHasCallback.Setup(x => x.BindDefault()).Verifiable();
                _optionThatHasCallback.Setup(x => x.Bind(_optionThatHasCallbackValue)).Verifiable();
                sut.Options.Add(_optionThatHasCallback.Object);

                // create option that has a callback and is required - the bind value should be executed like normal
                _optionThatIsRequired.SetupGet(x => x.IsRequired).Returns(true);
                _optionThatIsRequired.SetupGet(x => x.ShortName).Returns(_optionThatIsRequiredName);
                _optionThatIsRequired.Setup(x => x.Bind(_optionThatIsRequiredValue)).Verifiable();
                sut.Options.Add(_optionThatIsRequired.Object);

                // create blank option
                _blankOption.SetupGet(x => x.ShortName).Returns(_blankOptionName);
                _blankOption.Setup(x => x.Bind(_blankOptionValue)).Verifiable();
                sut.Options.Add(_blankOption.Object);

                var parserEngineResult = new Dictionary<string, string>
                {
                    {_optionThatHasCallbackName, _optionThatHasCallbackValue},
                    {_optionThatIsRequiredName, _optionThatIsRequiredValue},
                    {_blankOptionName, _blankOptionValue}
                };

                args = CreateArgsFromKvp(parserEngineResult);

                var parserEngineMock = new Mock<ICommandLineParserEngine>();
                parserEngineMock.Setup(x => x.Parse(args)).Returns(parserEngineResult);
                sut.ParserEngine = parserEngineMock.Object;
            };

            Because of = () => CatchAnyError(() => result = sut.Parse(args));

            It should_not_error = () => error.ShouldBeNull();

            It should_return_results_with_no_errors = () => result.Errors.ShouldBeEmpty();

            It should_return_no_unmatched_options = () => result.UnMatchedOptions.ShouldBeEmpty();

            It should_have_called_bind_on_the_option_has_callback_setup = () => _optionThatHasCallback.Verify(x => x.Bind(_optionThatHasCallbackValue), Times.Once());

            It should_have_called_bind_on_the_option_that_does_not_have_callback_setup = () => _blankOption.Verify(x => x.Bind(_blankOptionValue), Times.Once());

            It should_have_called_bind_on_the_option_that_is_required = () => _optionThatIsRequired.Verify(x => x.Bind(_optionThatIsRequiredValue), Times.Once());
        }
    }
}