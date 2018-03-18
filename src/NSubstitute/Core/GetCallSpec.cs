﻿using System;

namespace NSubstitute.Core
{
    public class GetCallSpec : IGetCallSpec
    {
        private readonly ICallCollection _callCollection;
        private readonly ICallSpecificationFactory _callSpecificationFactory;
        private readonly ICallActions _callActions;

        public GetCallSpec(ICallCollection callCollection, ICallSpecificationFactory callSpecificationFactory, ICallActions callActions)
        {
            _callCollection = callCollection;
            _callSpecificationFactory = callSpecificationFactory;
            _callActions = callActions;
        }

        public ICallSpecification FromPendingSpecification(MatchArgs matchArgs, PendingSpecificationInfo pendingSpecInfo)
        {
            if (pendingSpecInfo == null) throw new ArgumentNullException(nameof(pendingSpecInfo));

            return pendingSpecInfo.Handle(
                callSpec => FromExistingSpec(callSpec, matchArgs),
                lastCall =>
                {
                    _callCollection.Delete(lastCall);
                    return FromCall(lastCall, matchArgs);
                });
        }

        public ICallSpecification FromCall(ICall call, MatchArgs matchArgs)
        {
            return _callSpecificationFactory.CreateFrom(call, matchArgs);
        }

        public ICallSpecification FromExistingSpec(ICallSpecification spec, MatchArgs matchArgs)
        {
            return matchArgs == MatchArgs.AsSpecifiedInCall ? spec : UpdateCallSpecToMatchAnyArgs(spec);
        }

        ICallSpecification UpdateCallSpecToMatchAnyArgs(ICallSpecification callSpecification)
        {
            var anyArgCallSpec = callSpecification.CreateCopyThatMatchesAnyArguments();
            _callActions.MoveActionsForSpecToNewSpec(callSpecification, anyArgCallSpec);
            return anyArgCallSpec;
        }
    }
}