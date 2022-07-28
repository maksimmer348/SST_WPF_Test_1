using System.Collections.ObjectModel;

namespace SST_WPF_Test_1;

public class TestVipReport
{
    private ObservableCollection<Vip> testedVipReport;
    public void TestedReport(Vip testedVip)
    {
        testedVipReport.Add(testedVip);
        //TODO по окончанию или по ходу испытаний отсюда данные буду добавлятся в TelerikReport
    }
}