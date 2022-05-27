﻿using System;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var paymentProcessor = new InvoicePaymentProcessor();

            var payment = new Payment(0, "RANDOM_REFERENCE");
            var failureMessage = "";

            try
            {
                paymentProcessor.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var repo = new InvoiceRepository();

            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(0, reference);
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(0, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(10, reference, new Payment(10, reference));
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(0, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(10, reference, new Payment(5, reference));
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(6, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(5, reference);
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(6, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(10, reference, new Payment(5, reference));
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(5, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(10, reference, new Payment(10, reference));
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(10, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(10, reference, new Payment(5, reference));
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(1, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            
            var rnd = new Random();
            var reference = rnd.Next().ToString();
            var invoice = new Invoice(10, reference);
            repo.Add(invoice);

            var paymentProcessor = new InvoicePaymentProcessor(repo);

            var payment = new Payment(1, reference);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice is now partially paid", result);
        }
    }
}