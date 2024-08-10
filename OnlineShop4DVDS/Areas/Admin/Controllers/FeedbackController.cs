using DataAccess.Repository.IRepository;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Contact = Models.Contact;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]

    public class FeedbackController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IEmailSender _emailSender;

        public FeedbackController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Reply(int? id)
        {
            var contact = await _unitOfWork.ContactRepository.GetFirstOrDefault(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(Contact contact)
        {
            if (string.IsNullOrWhiteSpace(contact.Reply))
            {
                ModelState.AddModelError("Reply", "Reply cannot be empty");
                return View(contact);
            }

            using (var transaction = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    if (ModelState.IsValid)
                    {
                        var contactCheck = await _unitOfWork.ContactRepository.GetFirstOrDefault(c => c.Id == contact.Id);
                        if (contactCheck == null)
                        {
                            return NotFound();
                        }

                        // Update the reply in the database
                        contactCheck.Reply = contact.Reply;
                        contactCheck.IsSended = true;
                        _unitOfWork.ContactRepository.Update(contactCheck);
                        await _unitOfWork.Save();

                        // Send the email
                        string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reply to Your Feedback</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
        }}
        .header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 20px;
            text-align: center;
            border-radius: 8px 8px 0 0;
        }}
        .content {{
            margin: 20px 0;
            line-height: 1.6;
        }}
        .footer {{
            text-align: center;
            color: #888888;
            font-size: 12px;
            margin-top: 20px;
        }}
        .section {{
            margin-bottom: 20px;
        }}
        .section h2 {{
            font-size: 18px;
            color: #333333;
            margin-bottom: 10px;
        }}
        .section p {{
            font-size: 16px;
            color: #333333;
        }}
        .highlight {{
            background-color: #f8f9fa;
            border-left: 4px solid #007bff;
            padding: 10px;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Reply to Your Feedback</h1>
        </div>
        <div class='content'>
            <div class='section'>
                <p>Hi {contact.Name},</p>
                <p>Thank you for your feedback. We appreciate the time you took to share your thoughts with us. Below, you'll find our response to your feedback:</p>
            </div>
            <div class='section'>
                <h2>Your Feedback:</h2>
                <p class='highlight'>{contact.Feedback}</p>
            </div>
            <div class='section'>
                <h2>Our Reply:</h2>
                <p class='highlight'>{contact.Reply}</p>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} OnlineShop4DVDS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
                        await _emailSender.SendEmailAsync(contact.Email, "Reply to Your Feedback", emailBody);

                        // Commit the transaction
                        transaction.Complete();
                        TempData["swalsuccess"] = "Reply email have been send to customer";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(contact);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            var contact = await _unitOfWork.ContactRepository.GetFirstOrDefault(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }


        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contact = await _unitOfWork.ContactRepository.GetAll();
            return Json(new { data = contact });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var contactDeleted = await _unitOfWork.ContactRepository.GetFirstOrDefault(a => a.Id == id);
            if (contactDeleted == null)
            {
                return NotFound();
            }

            _unitOfWork.ContactRepository.Remove(contactDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Feedback deleted successfully" });
        }

        #endregion
    }
}
