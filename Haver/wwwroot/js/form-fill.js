
function fill1() {
    // Quality Representative Section
    $('input[name="QualityRepresentative.PartNo"]').val('123');
    $('input[name="QualityRepresentative.PoNo"]').val('456');
    $('input[name="QualityRepresentative.SalesOrd"]').val('SO789');
    $('input[name="QualityRepresentative.QuantReceived"]').val(100);
    $('input[name="QualityRepresentative.QuantDefective"]').val(5);
    $('input[name="QualityRepresentative.NonConforming"]').prop('checked', true);
    $('input[name="QualityRepresentative.QualityRepresentativeSign"]').val('John Doe');
    $('input[name="QualityRepresentative.QualityRepDate"]').val(getCurrentDate());
    $('textarea[name="QualityRepresentative.DescDefect"]').val('Defect description...');
    $('textarea[name="QualityRepresentative.DescItem"]').val('Item description...');
}

function fill2() {
    // Engineering Section
    $('input[name="Engineering.IsCustNotificationNecessary"]').prop('checked', true);
    $('input[name="Engineering.DrawReqUpdating"]').prop('checked', true);
    $('input[name="Engineering.OrgRevisionNum"]').val('123');
    $('input[name="Engineering.UpdatedRevisionNum"]').val('456');
    $('input[name="Engineering.RevisionedBy"]').val('Jane Smith');
    $('input[name="Engineering.RevisionDate"]').val(getCurrentDate());
    $('input[name="Engineering.EngineerSign"]').val('EngineerX');
    $('input[name="Engineering.EngineeringDate"]').val(getCurrentDate());
    $('textarea[name="Engineering.Disposition"]').val('Disposition details...');
    $('textarea[name="Engineering.CustIssueMsg"]').val('Customer issue message...');
}
function fill3() {
    // Operations Section
    $('input[name="Operations.CarRaised"]').prop('checked', true);
    $('input[name="Operations.IsFollowUpReq"]').prop('checked', true);
    $('input[name="Operations.CarNum"]').val('456');
    $('input[name="Operations.FollowUpType"]').val('FollowUpTypeXYZ');
    $('input[name="Operations.ExpecDate"]').val(getCurrentDate());
    $('input[name="Operations.OpManagerSign"]').val('ManagerZ');
    $('input[name="Operations.OperationsDate"]').val(getCurrentDate());
    $('textarea[name="Operations.Message"]').val('Operations message...');
}
function fill4() {
    // Procurement Section
    $('input[name="Procurement.SuppItemsBack"]').prop('checked', true);
    $('input[name="Procurement.SuppReturnCompleted"]').prop('checked', true);
    $('input[name="Procurement.ChargeSupplier"]').prop('checked', true);
    $('input[name="Procurement.IsCreditExpec"]').prop('checked', true);
    $('input[name="Procurement.RMANo"]').val('789');
    $('input[name="Procurement.ExpecDateOfReturn"]').val(getCurrentDate());
    $('input[name="Procurement.ProcurementSign"]').val('ProcurementLead');
    $('input[name="Procurement.ProcurementDate"]').val(getCurrentDate());
    $('textarea[name="Procurement.CarrierInfo"]').val('Carrier information...');
}
function fill5() {
    // Reinspection Section
    $('input[name="Reinspection.ReinspecAccepted"]').prop('checked', true);
    $('input[name="Reinspection.NCRClosed"]').prop('checked', true);
    $('input[name="Reinspection.NewNCRNum"]').val('456');
    $('input[name="Reinspection.ReinspecInspectorSign"]').val('InspectorY');
    $('input[name="Reinspection.QualityDPTSign"]').val('QualityDPTLead');
    $('input[name="Reinspection.ReinspectionDate"]').val(getCurrentDate());
    $('input[name="Reinspection.NCRClosedDate"]').val(getCurrentDate());
}