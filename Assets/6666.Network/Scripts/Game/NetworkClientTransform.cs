using Unity.Netcode.Components;

public class NetworkClientTransform : NetworkTransform
{
    // Ŭ���̾�Ʈ �� ���� ���װ� ��ȯ�� ����ȭ (ȣ��Ʈ ����)
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
