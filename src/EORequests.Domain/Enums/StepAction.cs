using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Domain.Enums
{
    public enum StepAction
    {
        View = 0,
        Act = 1,              // perform the step's action (approve/review/submit)
        UploadAttachment = 2,
        Comment = 3,
        CreateTask = 4,
        AssignStep = 5        // only when AssignmentMode.SelectedByPreviousStep
    }
}
