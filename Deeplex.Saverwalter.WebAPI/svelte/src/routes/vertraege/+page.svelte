<script lang="ts">
	import { WalterVertraege, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Verträge" />
<WalterVertraege search rows={data.rows} />
