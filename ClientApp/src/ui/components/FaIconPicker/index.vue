<script setup lang="ts">
defineOptions({
  name: 'FaIconPicker',
})

const props = withDefaults(defineProps<{
  modelValue?: string
  disabled?: boolean
}>(), {
  disabled: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const visible = defineModel<boolean>('visible', { default: false })

interface IconCollection {
  name: string
  prefix: string
  icons: string[]
}

// 图标集合 - 常用图标列表
const iconCollections: IconCollection[] = [
  {
    name: 'Heroicons',
    prefix: 'i-heroicons-solid',
    icons: [
      'home',
      'user',
      'users',
      'cog',
      'cog-6-to',
      'adjustments-horizontal',
      'adjustments-vertical',
      'archive-box',
      'archive-box-arrow-down',
      'archive-box-x-mark',
      'arrow-down',
      'arrow-left',
      'arrow-path',
      'arrow-right',
      'arrow-small-down',
      'arrow-small-left',
      'arrow-small-right',
      'arrow-small-up',
      'arrow-up',
      'arrow-uturn-down',
      'arrow-uturn-left',
      'arrow-uturn-right',
      'arrow-uturn-up',
      'backspace',
      'backtrack',
      'bars-3',
      'bars-3-bottom-left',
      'bars-3-center-left',
      'bars-4',
      'bell',
      'bell-alert',
      'bell-slash',
      'bell-snooze',
      'bookmark',
      'bookmark-slash',
      'bookmark-square',
      'book-open',
      'calendar',
      'calendar-days',
      'camera',
      'chart-bar',
      'chart-bar-square',
      'chart-pie',
      'chat-bubble-bottom-center',
      'chat-bubble-bottom-center-text',
      'chat-bubble-left',
      'chat-bubble-left-ellipsis',
      'chat-bubble-left-right',
      'check',
      'check-circle',
      'chevron-down',
      'chevron-left',
      'chevron-right',
      'chevron-up',
      'chevron-double-left',
      'chevron-double-right',
      'circle-stack',
      'clipboard',
      'clipboard-document',
      'clipboard-document-check',
      'clipboard-document-list',
      'clock',
      'cloud',
      'cloud-arrow-down',
      'cloud-arrow-up',
      'code-bracket',
      'cog',
      'cog-8-tooth',
      'collection',
      'color-swatch',
      'credit-card',
      'cube',
      'cube-transparent',
      'currency-dollar',
      'currency-euro',
      'currency-pound',
      'currency-yen',
      'cursor-arrow-rays',
      'document',
      'document-arrow-down',
      'document-arrow-up',
      'document-check',
      'document-duplicate',
      'document-text',
      'ellipsis-horizontal',
      'ellipsis-horizontal-circle',
      'ellipsis-vertical',
      'envelope',
      'envelope-open',
      'exclamation-circle',
      'exclamation-triangle',
      'eye',
      'eye-dropper',
      'eye-slash',
      'face-frown',
      'face-smile',
      'film',
      'finger-print',
      'fire',
      'flag',
      'folder',
      'folder-open',
      'funnel',
      'gift',
      'globe',
      'globe-alt',
      'hand-thumb-up',
      'hashtag',
      'heart',
      'home',
      'home-modern',
      'identification',
      'image',
      'information-circle',
      'inbox',
      'inbox-arrow-down',
      'inbox-stack',
      'key',
      'lifebuoy',
      'light-bulb',
      'link',
      'list-bullet',
      'lock-closed',
      'lock-open',
      'magnifying-glass',
      'magnifying-glass-plus',
      'magnifying-glass-minus',
      'map',
      'menu',
      'microphone',
      'minus',
      'minus-circle',
      'moon',
      'musical-note',
      'paper-airplane',
      'paper-clip',
      'pause',
      'pause-circle',
      'pencil',
      'pencil-square',
      'phone',
      'phone-arrow-down-left',
      'phone-arrow-up-right',
      'photo',
      'play',
      'play-circle',
      'plus',
      'plus-circle',
      'power',
      'printer',
      'puzzle-piece',
      'queue-list',
      'radio',
      'receipt',
      'rectangle-stack',
      'refresh',
      'rocket-launch',
      'rss',
      'scale',
      'scissors',
      'server',
      'share',
      'shield-check',
      'shield-exclamation',
      'shopping-bag',
      'shopping-cart',
      'signal',
      'sort-descending',
      'sparkles',
      'speaker-wave',
      'star',
      'stop',
      'stop-circle',
      'sun',
      'swatch',
      'table-cells',
      'tag',
      'terminal',
      'thumbs-down',
      'thumbs-up',
      'ticket',
      'translate',
      'trash',
      'truck',
      'tv',
      'user',
      'user-circle',
      'user-group',
      'user-minus',
      'user-plus',
      'users',
      'variable',
      'video-camera',
      'video-camera-slash',
      'viewfinder-circle',
      'wifi',
      'wrench',
      'wrench-screwdriver',
      'x-circle',
      'x-mark',
    ],
  },
  {
    name: 'Iconoir',
    prefix: 'i-iconoir',
    icons: [
      'add-circle',
      'add-file',
      'add-folder',
      'alert',
      'app-window',
      'archive',
      'arrow-down',
      'arrow-left',
      'arrow-right',
      'arrow-up',
      'attach',
      'back-space',
      'bag',
      'barcode',
      'battery',
      'bell',
      'block',
      'book',
      'book-page-minus',
      'book-page-plus',
      'bookmark',
      'books',
      'box',
      'brightness',
      'brush',
      'bucket',
      'bug',
      'building',
      'calendar',
      'camera',
      'cancel',
      'cart',
      'category',
      'check',
      'check-circle',
      'chevron-down',
      'chevron-left',
      'chevron-right',
      'chevron-up',
      'clock',
      'close',
      'close-circle',
      'cloud',
      'code',
      'copy',
      'credit-card',
      'crop',
      'cursor',
      'custom-file',
      'dash',
      'data-transfer',
      'delete',
      'device',
      'document',
      'download',
      'drag',
      'drop',
      'edit',
      'edit-pencil',
      'email',
      'eye-empty',
      'eye-off',
      'eye',
      'facebook',
      'facebook-tag',
      'fast-arrow-down',
      'fast-arrow-left',
      'fast-arrow-right',
      'fast-arrow-up',
      'feed-tag',
      'feed',
      'file',
      'filter',
      'flag',
      'flash',
      'folder',
      'fullscreen',
      'gif',
      'gift',
      'github',
      'google',
      'grid',
      'half-star',
      'half-moon',
      'half-sun',
      'half',
      'hashtag',
      'heart',
      'help',
      'home',
      'id-card',
      'image',
      'info',
      'instagram',
      'label',
      'lab-flask',
      'layers',
      'layout-grid',
      'light-bulb',
      'link',
      'list',
      'location',
      'lock',
      'log-in',
      'log-out',
      'magic-wand',
      'map',
      'menu',
      'mic',
      'mic-off',
      'minus',
      'moon',
      'more',
      'more-horiz',
      'more-vert',
      'nav-arrow-down',
      'nav-arrow-left',
      'nav-arrow-right',
      'nav-arrow-up',
      'navigate',
      'netflix',
      'news',
      'note',
      'notifications',
      'number',
      'options',
      'page',
      'pause',
      'pause-circle',
      'paypal',
      'phone',
      'photos',
      'picture',
      'pin',
      'pinterest',
      'play',
      'play-circle',
      'plus',
      'power',
      'print',
      'qr-code',
      'question',
      'radio-button',
      'record',
      'redo',
      'refresh',
      'remove',
      'remove-circle',
      'repeat',
      'reply',
      'restart',
      'retry',
      'right-arrow',
      'rss',
      'save',
      'scan',
      'scan-qr',
      'search',
      'send',
      'settings',
      'share',
      'shield-check',
      'shield',
      'shopping-bag',
      'shopping-cart',
      'sign-in',
      'sign-out',
      'sim-card',
      'slash',
      'slack',
      'snapchat',
      'snow',
      'sort',
      'sound-high',
      'sound-low',
      'sound-mute',
      'sound',
      'space-bar',
      'star',
      'stop',
      'stop-circle',
      'store',
      'stumbleupon',
      'sun',
      'support',
      'sync',
      'system',
      'tab',
      'tag',
      'target',
      'telegram',
      'terminal',
      'thumb-down',
      'thumb-up',
      'ticket',
      'tiktok',
      'time',
      'timer',
      'toggle-off',
      'toggle-on',
      'tools',
      'trash',
      'tumblr',
      'twitter',
      'undo',
      'unlink',
      'unlock',
      'up-arrow',
      'update',
      'upload',
      'user',
      'verified',
      'video',
      'view',
      'vimeo',
      'volume-high',
      'volume-low',
      'volume-mute',
      'vk',
      'warning',
      'whatsapp',
      'wifi',
      'window',
      'wrench',
      'x',
      'x-circle',
      'xmark',
      'youtube',
      'zoom-in',
      'zoom-out',
    ],
  },
]

const selectedCollectionPrefix = ref(iconCollections[0].prefix)
const searchQuery = ref('')

const selectedCollection = computed(() => {
  return iconCollections.find(collection => collection.prefix === selectedCollectionPrefix.value) ?? iconCollections[0]
})

// 当前选中的图标
const currentIcon = computed({
  get: () => props.modelValue,
  set: val => emit('update:modelValue', val || ''),
})

// 过滤后的图标列表
const filteredIcons = computed(() => {
  if (!searchQuery.value) {
    return selectedCollection.value.icons
  }
  const query = searchQuery.value.toLowerCase()
  return selectedCollection.value.icons.filter(icon =>
    icon.toLowerCase().includes(query),
  )
})

// 选择图标
function selectIcon(icon: string) {
  currentIcon.value = `${selectedCollection.value.prefix}:${icon}`
  visible.value = false
}

// 清除图标
function clearIcon() {
  currentIcon.value = ''
  visible.value = false
}

// 转换为 UnoCSS 格式的图标名（用于预览）
function formatIconName(icon: string): string {
  return `${selectedCollection.value.prefix}:${icon}`
}

// 获取当前图标的显示名称
/* eslint-disable style/max-statements-per-line */
function getCurrentIconLabel(): string {
  if (!currentIcon.value) { return '请选择图标' }
  return currentIcon.value
}
/* eslint-enable style/max-statements-per-line */
</script>

<template>
  <div class="fa-icon-picker">
    <el-input
      :model-value="getCurrentIconLabel()"
      placeholder="请选择图标"
      readonly
      :disabled="disabled"
      @click="!disabled && (visible = true)"
    >
      <template #prefix>
        <FaIcon v-if="currentIcon" :name="currentIcon" class="text-lg" />
        <FaIcon v-else name="i-iconoir:picture" class="text-lg text-gray-400" />
      </template>
      <template #suffix>
        <FaIcon name="i-iconoir:nav-arrow-down" class="text-gray-400" />
      </template>
    </el-input>

    <el-dialog
      v-model="visible"
      title="选择图标"
      width="700px"
      :close-on-click-modal="true"
      append-to-body
    >
      <!-- 分类选择 -->
      <div class="mb-4">
        <el-radio-group v-model="selectedCollectionPrefix" size="small">
          <el-radio-button
            v-for="collection in iconCollections"
            :key="collection.prefix"
            :value="collection.prefix"
          >
            {{ collection.name }}
          </el-radio-button>
        </el-radio-group>
      </div>

      <!-- 搜索框 -->
      <div class="mb-4">
        <el-input
          v-model="searchQuery"
          placeholder="搜索图标..."
          clearable
        >
          <template #prefix>
            <FaIcon name="i-iconoir:search" />
          </template>
        </el-input>
      </div>

      <!-- 图标列表 -->
      <div class="icon-list-container">
        <div
          v-for="icon in filteredIcons"
          :key="icon"
          class="icon-item"
          :class="{ active: currentIcon === formatIconName(icon) }"
          @click="selectIcon(icon)"
        >
          <div class="icon-wrapper">
            <FaIcon :name="formatIconName(icon)" class="icon" />
          </div>
          <span class="icon-name">{{ icon }}</span>
        </div>
        <div v-if="filteredIcons.length === 0" class="empty-text">
          没有找到匹配的图标
        </div>
      </div>

      <template #footer>
        <el-button @click="clearIcon">
          清除
        </el-button>
        <el-button type="primary" @click="visible = false">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.fa-icon-picker :deep(.el-input__wrapper) {
  cursor: pointer;
}

.fa-icon-picker :deep(.el-input__wrapper .el-input__inner) {
  cursor: pointer;
}

.icon-list-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(90px, 1fr));
  gap: 8px;
  max-height: 400px;
  overflow-y: auto;
  padding: 8px;
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  background-color: var(--el-bg-color-page);
}

.icon-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10px 6px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.icon-item:hover {
  background-color: var(--el-fill-color-light);
  border-color: var(--el-border-color);
}

.icon-item.active {
  background-color: var(--el-color-primary-light-9);
  border-color: var(--el-color-primary);
}

.icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  margin-bottom: 4px;
}

.icon-item .icon {
  font-size: 24px;
  color: var(--el-text-color-primary);
}

.icon-item.active .icon {
  color: var(--el-color-primary);
}

.icon-name {
  font-size: 11px;
  color: var(--el-text-color-secondary);
  text-align: center;
  word-break: break-all;
  line-height: 1.2;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.empty-text {
  grid-column: 1 / -1;
  text-align: center;
  padding: 40px;
  color: var(--el-text-color-secondary);
}
</style>
